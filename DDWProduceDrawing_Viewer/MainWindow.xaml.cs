using DINNO.HU3D.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace DDWProduceDrawing_Viewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string db_path = "C:\\Users\\Dinno\\Downloads\\24.08.05 (H2) ELPGX13_김\\";
            string path = System.IO.Path.GetDirectoryName(db_path);
            string name = String.Empty;
            DirectoryInfo files = new DirectoryInfo(path);
            foreach (FileInfo file in files.GetFiles())
            {
                if (file.Extension.ToLower().CompareTo(".dpf") == 0)
                {
                    name = file.FullName;
                }
            }
            ProjectBase projectBase = DINNO.HU3D.Workspace.StandaloneProject.From(name) as StandaloneProject;
            AppWorkspace a = AppWorkspace.newInstance(projectBase);
            bool b = AppWorkspace.Instance.Open(projectBase.DBFileName);
            
            label.Content = b.ToString();
            

        }
    }
}
