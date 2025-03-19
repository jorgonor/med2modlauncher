using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Reflection;

namespace Med2ModLauncher
{
  
    public partial class Form1 : Form
    {
        private static readonly string REGISTRY_KEY = @"HKEY_CURRENT_USER\SOFTWARE\Med2ModLauncher";
        private static readonly List<String> BUILT_IN_MODS = new List<String>()
        {
           "americas", "british_isles", "crusades", "teutonic"
        };

        private static readonly string[] PROGRAM_FILES_FOLDERS = new string[] { "Program Files (x86)", "Program Files" };
        private static readonly string GAME_STEAM_ROOT = "Steam\\steamapps\\common\\Medieval II Total War";
        private static readonly string MODS_FOLDER = "mods";
        private static readonly string  MEDIEVAL_EXE = "medieval2.exe";

        private List<String> mods = new List<String>();

        private string med2Root = null;
        private string modsFullPath = null;

        public Form1()
        {
            InitializeComponent();
            LookForMed2Root();
            ReadAvailableMods();
        }

        private void LookForMed2Root()
        {
            string registryFolder = GetRegistryFolder();
            
            if (registryFolder == null)
            {
                med2Root = TryToFindMed2Root();
            }
            else
            {
                if (ValidateMed2Root(registryFolder))
                {
                    med2Root = registryFolder;
                }
            }

            if (med2Root != null)
            {
                if (!ValidateMed2Root(med2Root))
                {
                    OnWrongPath();
                }
                else
                {
                    modsFullPath = GetModsFullPath();
                    SetRegistryFolder(med2Root);
                }
            }
            else
            {
                MessageBox.Show("It hasn't been possible to find Medieval2 installed automatically. Please, select a folder using Tools > Change Medieval II Folder.");
            }
        }

       
        private bool SetMed2Root(string path)
        {
            if (ValidateMed2Root(path))
            {
                med2Root = path;
                modsFullPath = GetModsFullPath();
                SetRegistryFolder(path);
                return true;
            }
            else
            {
                OnWrongPath();
            }
            return false;
        }

        private void ReadAvailableMods()
        {
            if (modsFullPath != null)
            {
                ImageList imageList = CreateImageList();
                listView1.LargeImageList = imageList;

                DirectoryInfo directoryInfo = new DirectoryInfo(modsFullPath);

                foreach (DirectoryInfo child in directoryInfo.GetDirectories())
                {
                    mods.Add(child.Name);

                    int modIndex = BUILT_IN_MODS.IndexOf(child.Name);

                    if (modIndex >= 0)
                    {
                        listView1.Items.Add(child.Name, modIndex + 1);
                    }
                    else
                    {
                        FileInfo[] possibleIcons = child.GetFiles("*.ico");
                        if (possibleIcons.Length == 0)
                        {
                            listView1.Items.Add(child.Name, 0);
                        }
                        else
                        {
                            listView1.LargeImageList.Images.Add(child.Name, Image.FromFile(possibleIcons[0].FullName));
                            listView1.Items.Add(child.Name, listView1.LargeImageList.Images.Count - 1);
                        }
                    }
                }
            }
            else
            {
                if (listView1.Items != null) {
                    listView1.Items.Clear();
                }
                if (listView1.LargeImageList != null && listView1.LargeImageList.Images != null) {
                    listView1.LargeImageList.Images.Clear();
                }
                
            }
        }

        private string GetModsFullPath() {
            return med2Root + "\\" + MODS_FOLDER;
        }

        private void ClearMed2Paths()
        {
            modsFullPath = null;
            med2Root = null;
        }

        private void OnWrongPath()
        {
            WarnWrongPath();
            ClearMed2Paths();
        }

        private void WarnWrongPath()
        {
            MessageBox.Show("The selected folder " + med2Root + " is not a Medieval II kingdoms folder. Please, select another using Tools > Change Medieval II Folder.");
        }


        private ImageList CreateImageList()
        {
            ImageList imageList = new ImageList();

            var assembly = Assembly.GetExecutingAssembly();

            imageList.ImageSize = new Size(64, 64);

            imageList.Images.Add("med2", Image.FromStream(assembly.GetManifestResourceStream("Med2ModLauncher.icons.med2.ico")));
            imageList.Images.Add("americas", Image.FromStream(assembly.GetManifestResourceStream("Med2ModLauncher.icons.americas.png")));
            imageList.Images.Add("british_isles", Image.FromStream(assembly.GetManifestResourceStream("Med2ModLauncher.icons.british_isles.png")));
            imageList.Images.Add("crusades", Image.FromStream(assembly.GetManifestResourceStream("Med2ModLauncher.icons.crusades.png")));
            imageList.Images.Add("teutonic", Image.FromStream(assembly.GetManifestResourceStream("Med2ModLauncher.icons.teutonic.png")));
            

            return imageList;
        }

        private string GetRegistryFolder()
        {
            object registryX = Registry.GetValue(REGISTRY_KEY, "folder", "NULL");
            string registryValue = null;
            if (registryX != null)
            {
                registryValue = registryX.ToString();
            }

            if (registryValue == null)
            {
                return null;
            }

            return  !registryValue.Equals("NULL") ? registryValue : null;
        }

        private void SetRegistryFolder(string path)
        {
            Registry.SetValue(REGISTRY_KEY, "folder", path);
        }

        private void ExitForm()
        {
            if (System.Windows.Forms.Application.MessageLoop)
            {
                // WinForms app
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                // Console app
                System.Environment.Exit(1);
            }
        }

        private bool ValidateMed2Root(string path)
        {
            bool modsDirectoryExists = false, thereIsAMedieval2Exe = false;

            DirectoryInfo directoryInfo = new DirectoryInfo(path + "\\" + MODS_FOLDER);
            modsDirectoryExists = directoryInfo.Exists;

            FileInfo fileInfo = new FileInfo(path + "\\" + MEDIEVAL_EXE);
            thereIsAMedieval2Exe = fileInfo.Exists;

            return modsDirectoryExists && thereIsAMedieval2Exe;
        }

        private string TryToFindMed2Root()
        {
            string[] drives = System.Environment.GetLogicalDrives();
            string aux;
            foreach (string drive in drives)
            {
                foreach (string programFilesFolder in PROGRAM_FILES_FOLDERS)
                {
                    aux = drive + programFilesFolder + "\\" + GAME_STEAM_ROOT;
                    DirectoryInfo directoryInfo = new DirectoryInfo(aux);
                    if (directoryInfo.Exists)
                    {
                        return aux;
                    }
                }
            }

            return null;
        }

        // Events

        private void OnLaunch(object sender, EventArgs e)
        {
            System.Collections.IEnumerator enumerator = listView1.SelectedItems.GetEnumerator();

            enumerator.MoveNext();

            object current = enumerator.Current;

            if (current != null) { 

                string selectedMod = ((ListViewItem)current).Text;

                string modFolder = 
                med2Root + Path.DirectorySeparatorChar
                 + "mods" + Path.DirectorySeparatorChar 
                 + selectedMod
                 + Path.DirectorySeparatorChar;

                string eopV2exe = 
                 modFolder
                 + Path.DirectorySeparatorChar
                 + "M2TWEOP_GUI.exe";

                string eopV1exe = 
                 modFolder
                 + Path.DirectorySeparatorChar
                 + "M2TWEOP.exe";        

                bool eopV2Exists = File.Exists(eopV2exe);
                bool eopV1Exists = File.Exists(eopV1exe);

                Process process = new Process();

                // If the mod has an EOP V1 (M2TWEOP.exe) or EOP V2(M2TWEOP_GUI.exe), use it
                if (eopV2Exists)
                {
                    process.StartInfo.FileName = eopV2exe;
                    process.StartInfo.WorkingDirectory = modFolder;
                }
                else if (eopV1Exists)
                {
                    process.StartInfo.FileName = eopV1exe;
                    process.StartInfo.WorkingDirectory = modFolder;
                }
                else
                {
                    process.StartInfo.FileName = med2Root + "\\" + MEDIEVAL_EXE;
                    process.StartInfo.WorkingDirectory = med2Root;
                    process.StartInfo.Arguments = "--features.mod=mods/" + selectedMod + " --io.file_first";
                }

                process.Start();
            }
        }

        private void CreditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created by LordJorgonor.");
        }

        private void ChangeFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();
            this.Focus();
            
            if (result == DialogResult.OK)
            {
                SetMed2Root(folderDialog.SelectedPath);
                ReadAvailableMods();
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ExitForm();
        }


    }
}
