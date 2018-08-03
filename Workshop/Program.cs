using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using Newtonsoft.Json;
using System.IO;

namespace Workshop
{
    class Program
    {

        protected static CallResult<CreateItemResult_t> m_itemCreated;
        protected static CallResult<SubmitItemUpdateResult_t> m_itemSubmitted;
        protected static UGCUpdateHandle_t ugcUpdateHandle = UGCUpdateHandle_t.Invalid;

        protected static ulong publishID = 0;


        static void Main(string[] args)
        {

            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }
            var configPath = args[0];
            var file = File.ReadAllText(configPath);
            var package = JsonConvert.DeserializeObject<Package>(file);

            m_itemCreated = CallResult<CreateItemResult_t>.Create(OnItemCreated);

            var appID = "798840";

            var AppId_t = new AppId_t(uint.Parse(appID));

            var isInit = SteamAPI.Init();

            if (package.publishFileID == 0)
            {
                SteamAPICall_t call = SteamUGC.CreateItem(new AppId_t(uint.Parse(appID)), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                m_itemCreated.Set(call);
            }
            else
            {
                publishID = package.publishFileID;
            }
            while (publishID == 0)
            {
                SteamAPI.RunCallbacks();
                Thread.Sleep(1000);
            }


            Thread.Sleep(1000);

            if (package.publishFileID == 0)
            {
                package.publishFileID = publishID;
                File.WriteAllText(configPath, JsonConvert.SerializeObject(package));
                Console.WriteLine(string.Format("Rewrite package ID with {0}",publishID));
            }
            var publishFileID_t = new PublishedFileId_t(publishID);

            ugcUpdateHandle = SteamUGC.StartItemUpdate(AppId_t, publishFileID_t);

            SteamUGC.SetItemTitle(ugcUpdateHandle, package.title);
            SteamUGC.SetItemDescription(ugcUpdateHandle, package.description);
            SteamUGC.SetItemVisibility(ugcUpdateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
            SteamUGC.SetItemTags(ugcUpdateHandle, package.tags);
            SteamUGC.SetItemPreview(ugcUpdateHandle, Directory.GetCurrentDirectory() + "/" + package.previewUrl);
            SteamUGC.SetItemContent(ugcUpdateHandle, Directory.GetCurrentDirectory() + "/" + package.contentUrl);

            SteamAPICall_t t = SteamUGC.SubmitItemUpdate(ugcUpdateHandle, "Update file from game tool");

            m_itemSubmitted = CallResult<SubmitItemUpdateResult_t>.Create(OnItemSubmitted);

            m_itemSubmitted.Set(t);

            while (true)
            {
                Thread.Sleep(1000);

                if (ugcUpdateHandle == UGCUpdateHandle_t.Invalid)
                {
                    break;
                }

                SteamAPI.RunCallbacks();

                ulong bytesDone, bytesTotal;

                EItemUpdateStatus status = SteamUGC.GetItemUpdateProgress(ugcUpdateHandle, out bytesDone, out bytesTotal);

                ProgressPrint(string.Format("status:{0} bytesDone:{1} bytesTotal:{2}", status, bytesDone, bytesTotal));
            }
            Console.WriteLine("Everything is ready !Check it on your workshop.");

            Console.WriteLine("Be sure to set the item public so that the community can download it!");

            Console.WriteLine("Press any key to quit");

            Console.ReadKey();

            SteamAPI.Shutdown();
        }

        private static void ProgressPrint(string content)
        {
            Console.SetCursorPosition(0, 5);
            Console.WriteLine(content);
        }

        private static void OnItemSubmitted(SubmitItemUpdateResult_t param, bool bIOFailure)
        {
            if (bIOFailure)
            {
                Console.WriteLine("Error: I/O Failure! :(");
                return;
            }

            switch (param.m_eResult)
            {
                case EResult.k_EResultOK:
                    Console.WriteLine("SUCCESS! Item submitted! :D :D :D");
                    ugcUpdateHandle = UGCUpdateHandle_t.Invalid;
                    break;
            }
        }



        private static void OnItemCreated(CreateItemResult_t callBack, bool bIOFailure)
        {
            if (bIOFailure)
            {
                Console.WriteLine("Error: I/O Failure! :(");
                return;
            }

            switch (callBack.m_eResult)
            {
                case EResult.k_EResultInsufficientPrivilege:
                    // you're banned!
                    Console.WriteLine("Error: Unfortunately, you're banned by the community from uploading to the workshop! Bummer. :(");
                    break;
                case EResult.k_EResultTimeout:
                    Console.WriteLine("Error: Timeout :S");
                    break;
                case EResult.k_EResultNotLoggedOn:
                    Console.WriteLine("Error: You're not logged into Steam!");
                    break;
            }

            if (callBack.m_eResult == EResult.k_EResultOK)
            {
                var callback_publishID = callBack.m_nPublishedFileId;
                publishID = ulong.Parse(callback_publishID.ToString());
                Console.WriteLine(callback_publishID);

            }
        }
    }
}
