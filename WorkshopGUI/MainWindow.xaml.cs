using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Workshop;
using Newtonsoft.Json;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace WorkshopGUI
{
    public class ModFile
    {
        public ulong FileID { get; set; }
        public string FileName { get; set; }
        public string FullName { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Package currentPackage = new Package();

        private OpenFileDialog openFileDialog = new OpenFileDialog();
        private FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

        private ObservableCollection<ModFile> modFiles = new ObservableCollection<ModFile>();

        public string currentEditFileName = "new_package.json";

        public List<string> packageType = new List<string>()
        {
            "Skin","Map"
        };


        public MainWindow()
        {
            openFileDialog.Filter = "Picture|*.png";
            openFileDialog.InitialDirectory = System.Windows.Forms.Application.StartupPath;

            openFolderDialog.SelectedPath = System.Windows.Forms.Application.StartupPath;

            InitializeComponent();

            var notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath),
                BalloonTipText = "Workshop uploader"
            };
            notifyIcon.ShowBalloonTip(3000);
            notifyIcon.Visible = true;

            typeComboBox.ItemsSource = packageType;
            fileList.ItemsSource = modFiles;

            UpdateFileList();

            SelectFile();
        }

        private void UpdateFileList()
        {
            modFiles.Clear();

            var dir = new DirectoryInfo(System.Windows.Forms.Application.StartupPath + "/Mods/");
            var files = dir.GetFiles("*.json");

            foreach (var file in files)
            {
                var str = File.ReadAllText(file.FullName);
                var pack = JsonConvert.DeserializeObject<Package>(str);

                modFiles.Add(new ModFile()
                {
                    FileName = file.Name,
                    FullName = file.FullName,
                    FileID = pack.publishFileID
                });
            }
        }
        #region Package Config UI
        private void TitleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            currentPackage.title = titleTextBox.Text;
        }

        private void DescriptionTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            currentPackage.description = descriptionTextBox.Text;
        }

        private void typeComboBox_Selected(object sender, RoutedEventArgs e)
        {
            currentPackage.tags = packageType[typeComboBox.SelectedIndex];
        }

        private void PickPreviewPic(object sender, RoutedEventArgs e)
        {
            var done = openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;

            if (done)
            {
                Uri fullPath = new Uri(openFileDialog.FileName, UriKind.Absolute);
                Uri relRoot = new Uri(System.Windows.Forms.Application.ExecutablePath, UriKind.Absolute);

                previewImg.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                currentPackage.previewUrl = relRoot.MakeRelativeUri(fullPath).ToString();
            }
        }


        private void PickResFolder(object sender, RoutedEventArgs e)
        {
            var done = openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;

            if (done)
            {
                Uri fullPath = new Uri(openFolderDialog.SelectedPath, UriKind.Absolute);
                Uri relRoot = new Uri(System.Windows.Forms.Application.ExecutablePath, UriKind.Absolute);

                contentUrlLabel.Content = relRoot.MakeRelativeUri(fullPath).ToString();
                currentPackage.contentUrl = relRoot.MakeRelativeUri(fullPath).ToString();
            }

        }
        #endregion

        private void UploadPackage(object sender, RoutedEventArgs e)
        {
            if (currentPackage.title == "")
            {
                System.Windows.MessageBox.Show("Please set title", "Null");
                return;
            }

            if (currentPackage.description == "")
            {
                System.Windows.MessageBox.Show("Please set description", "Null");
                return;
            }

            if (currentPackage.previewUrl == "")
            {
                System.Windows.MessageBox.Show("Please select preview image", "Null");
                return;
            }

            if (currentPackage.contentUrl == "")
            {
                System.Windows.MessageBox.Show("Please select mod folder to upload", "Null");
                return;
            }

            var json = JsonConvert.SerializeObject(currentPackage);

            File.WriteAllText(string.Format("Mods/{0}", currentEditFileName), json);
   
            UpdateFileList();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "Workshop.exe",
                Arguments = string.Format("Mods/{0}", currentEditFileName),
                WindowStyle = ProcessWindowStyle.Normal
            };
            Process.Start(startInfo);
        }

        private void AddModFileJson(object sender, RoutedEventArgs e)
        {
            CreateNewJson();
        }

        private void CreateNewJson()
        {
            var package = new Package()
            {
                publishFileID = 0,
                title = "My mod title",
                description = "My description",
                tags = currentPackage.tags,
                previewUrl = "/SteamWork.png",
                contentUrl = "/"
            };

            var json = JsonConvert.SerializeObject(package);

            File.WriteAllText(string.Format("Mods/{0}.json", newFileTextBox.Text), json);

            UpdateFileList();
        }

        private void UpdateUI(Package package)
        {
            currentPackage = package;

            titleTextBox.Text = package.title;

            descriptionTextBox.Text = package.description;
            try
            {
                previewImg.Source = new BitmapImage(new Uri(System.Windows.Forms.Application.StartupPath + "/"+package.previewUrl));
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Fail to load preview img");
            }


            contentUrlLabel.Content = package.contentUrl;

            typeComboBox.SelectedIndex = packageType.IndexOf(package.tags);
        }

        private void OnFileSelected(object sender, MouseButtonEventArgs e)
        {
            SelectFile();
        }

        private void SelectFile()
        {
            if (modFiles.Count <= 0)
            {
                return;
            }
        
            var index = fileList.SelectedIndex<0|| fileList.SelectedIndex > modFiles.Count-1 ? 0 : fileList.SelectedIndex;
        
            var file = modFiles[index];
            currentEditFileName = file.FileName;

            var str = File.ReadAllText(file.FullName);
            var pack = JsonConvert.DeserializeObject<Package>(str);

            currentEditLabel.Content = "Current Edit:"+ file.FileName;
            UpdateUI(pack);
        }
    }
}
