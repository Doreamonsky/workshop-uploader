using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string installDir = "../dlc/PanzerWar/mods/Installs/";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnInstallButtonClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Mod(*.modpack)|*.modpack|Paid mod(*.umodpack)|*.umodpack"
            };

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var dir = new DirectoryInfo(installDir);

                if (!dir.Exists)
                {
                    dir.Create();
                }

                foreach (var fileName in fileDialog.FileNames)
                {
                    var fileInfo = new FileInfo(fileName);

                    File.Copy(fileInfo.FullName, $"{installDir}{fileInfo.Name}", true);
                }
            }
        }

        private void OnModFolderButtonClick(object sender, RoutedEventArgs e)
        {
            var dir = new DirectoryInfo(installDir);

            if (dir.Exists)
            {
                Process.Start(dir.FullName);
            }
        }
    }
}
