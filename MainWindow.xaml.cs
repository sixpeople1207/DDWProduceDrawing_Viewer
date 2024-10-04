using DINNO.HU3D.Workspace;
using DINNO.HU3D.WPF;
using DINNO.HU3D.Workspace.Tools;
using DINNO.HU3D.Workspace.ProdcutionDrawing;
using DINNO.DO3D.MEP.Instances;
using DINNO.DO3D.MEP;
using DINNO.DO3D.SceneGraph.Graphics.Platform.InputHandler.HUDInputHandler;
using DINNO.DO3D.SceneGraph.Graphics;
using DINNO.DO3D.SceneGraph.Graphics.Platform.InputHandler;
using DINNO.DO3D.MEP.Visualizer;
using DINNO.DO3D.SceneGraph.Graphics.Scene.Viewing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using DINNO.DO3D.SceneGraph.Graphics.Scene.Renderer;
using DINNO.DO3D.IO.Common;
using DevExpress.Xpf.Core;
using System.Windows.Controls;
using DINNO.DO3D.IO.Instances;
using DDWProduceDrawing_Viewer.UI.FluentSplashScreen;
using DINNO.DO3D.CLIENT.IO.Comm;

using static DINNO.DO3D.MEP.MEPIndexMapSceneViewController;
using static DINNO.DO3D.MEP.HookUpSceneVisualizer;
using DevExpress.XtraCharts;
using DINNO.HU3D.WPF.HookUpDesigner;
using DINNO.HU3D.Workspace.ProductionDrawing;
using DINNO.DO3D.IO.DataModel;
using DINNO.DO3D.IO.DataModel.InstanceDataModel;
using DINNO.DO3D.SceneGraph.Graphics.Math;
using DINNO.HU3D.UI.WPF.Drawings;
using DINNO.HU3D.UI.WPF;
using DevExpress.Utils.Extensions;
using DINNO.DO3D.IO.Spaces;
using DevExpress.Xpf.Grid;
using DINNO.DO3D.Database.DataModel;
using DINNO.HU3D.ViewModel.STD;
using DevExpress.Data.Filtering.Helpers;
using System.Collections.ObjectModel;


namespace DDWProduceDrawing_Viewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>d
    public partial class MainWindow : Window
    {
        MEPHookUpSceneViewController _sceneViewController;
        MEPSceneViewControlWPF _sceneViewControl;
        MEPSceneDataSource _DataSource = null;

        public MainWindow()
        {
            InitializeComponent();
            string db_path = @"./Resources/Dinno Elec.dpf";
            string name = String.Empty;
            //DirectoryInfo files = new DirectoryInfo(path);
            //foreach (FileInfo file in files.GetFiles())
            //{
            //    if (file.Extension.ToLower().CompareTo(".dpf") == 0)
            //    {
            //        name = file.FullName; break;
            //    }
            //}
            //try
            //{
            //    DO3DContext.getInstance().checkLicenseKey("4AEB810");
            //    ProjectBase projectBase = DINNO.HU3D.Workspace.StandaloneProject.From(@"./Resources/Dinno Elec.dpf") as StandaloneProject;
            //    AppWorkspace a = AppWorkspace.newInstance(projectBase);
            //    AppWorkspace.releaseInstance();
            //    bool b = AppWorkspace.Instance.Open(projectBase.DBFileName);

            //    AppWorkspace.Instance.LoadData(WorkspaceModuleType.HookupDesigner);
            //    label.Content = b.ToString();

            //    _sceneViewController = new MEPSceneViewController();
            //    _sceneViewControl = new MEPSceneViewControlWPF();
            //    _sceneViewControl.setController(_sceneViewController);
            //    _sceneViewControl.SetSize(new System.Windows.Size(_clientViewGrid.ActualHeight, _clientViewGrid.ActualWidth));
            //    _sceneViewController.DataSource = AppWorkspace.Instance.MEPDataSource;
            //    _sceneViewController.BackgroundColor.set(AppConfiguration.HookupBackgroundColor);

            //    _clientViewGrid.Children.Add(_sceneViewControl);
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show(e.ToString());
            //}
            ProjectBase project;
            string key = "4AEB810";
            AppWorkspace.SetLicense(key, out string message);
            string path_2 = "C:\\Users\\Dinno\\Downloads\\24.08.05 (H2) ELPGX13_김\\ELPGX13.dpf";

            File.Exists(path_2);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path_2);
            project = DINNO.HU3D.Workspace.StandaloneProject.From(path_2) as StandaloneProject;
            AppWorkspace app = AppWorkspace.newInstance(project);
            var instance = openWorkspace(project);
            AppWorkspace.Instance.Open(project.DBFileName);
            AppWorkspace.Instance.LoadData(WorkspaceModuleType.HookupDesigner);
            AppWorkspace.Instance.setCurrentMode(DINNO.DO3D.IO.Spaces.CurrentViewModeType.Building, AppWorkspace.Instance.SiteManager.SiteList[0].BuildingList[0].BuildingID);
            Dictionary<Guid, string> _allGroup = AppWorkspace.Instance.InstanceGroupManager.getGroupTextMap();
            string buldingName = AppWorkspace.Instance.SiteManager.SiteList[0].BuildingList[0].BuildingName;
            
            _DataSource = HookUpWorkspace.Instance.DataSource;
            List<Guid> instances_List = new List<Guid>();
            //_instanceGroupOC = new InstanceGroupOC(); //conn에러
            Guid currentGroup = new Guid();
            Guid[] instances =  new Guid[_allGroup.Count];
            foreach (var group in _allGroup)
            {
                if (group.Value.Contains(buldingName) && group.Value.Contains("진공"))
                {
                    currentGroup = group.Key;
                    instances = AppWorkspace.Instance.InstanceGroupManager.getInstanceIdListFromGroup(group.Key);
                    if(instances.Length > 0)
                    {
                        foreach (var ins in instances) {
                            instances_List.Add(ins);
                        }
                    }
                }
            }
            initScene();
            zoomFit(instances_List);
            //CreateTree();
            AppWorkspace.Instance.ProductionDrawingManager.Start(currentGroup, instances_List);
            AppWorkspace.Instance.loadHookupUtil();
            ProductionDrawingManager _product = AppWorkspace.Instance.ProductionDrawingManager;
            ProductionDrawingGroupModel[] sGroup =  _product.SpoolGroups;
            

            _sceneViewController.setSoloview(instances);
            _sceneViewController.setViewingMode(ViewingController.ViewingTypes.Perspective);
        }
        private void initScene()
        {
            _sceneViewController = new MEPHookUpSceneViewController();
            _sceneViewControl = new MEPSceneViewControlWPF();
            _sceneViewControl.setController(_sceneViewController);
        
            //string key = Properties.Settings.Default.LicenseKey;
            DO3DContext.getInstance().checkLicenseKey("4AEB810");
           
            //_sceneViewControl.SetSize(new System.Windows.Size(_clientViewGrid.ActualWidth, _clientViewGrid.ActualHeight));
            _sceneViewControl.Height = 800.0;
            _sceneViewControl.Width = 800.0;
            AppWorkspace.Instance.MEPDataSource.FarHookUpDistance = 30000;
            _sceneViewController.DataSource = AppWorkspace.Instance.MEPDataSource;
            _sceneViewController.PolygonMode = PolygonModes.Fill;
            _sceneViewController.ShadingMode = HookUpSceneVisualizer.ShadingModes.Utiltity;
            _sceneViewController.BackgroundColor.set(AppConfiguration.HookupBackgroundColor);
            _sceneViewController.LClickFunc += selectObject;
            _sceneViewController.setNearFar(10, 50000);
            HUDCubePerspectiveInputHandler c2 = new HUDCubePerspectiveInputHandler(AppConfiguration.ViewCubePixelSize, 0);
            
            c2.setDock(HUDInputHandlerBase.DockingPositions.RightTop);
            c2.Visible = true;
            _sceneViewControl.InputHandlers.addHud(c2);
            
            _clientViewGrid.Children.Add(_sceneViewControl);
            //LoadingScreen loadingScreen = new LoadingScreen();
            //loadingScreen.ShowSplashScreen();
        }
        private void selectObject(DINNO.DO3D.SceneGraph.Platform.MouseEventArgs args)
        {
            var obj = _sceneViewController.pickNearestObject(args.Location);
            if (obj == null)
                AppWorkspace.Instance.SelectedInstanceManager.clear(null);
            else
                AppWorkspace.Instance.SelectedInstanceManager.addSelectInstance(obj.GID, null, true);
        }
     
        //HookupDesigner.GroupCommandUI
        private void zoomFit(List<Guid> guids)
        {
            try
            {
                AABB bb = AppWorkspace.Instance.MEPDataSource.getCurrentBoundary();
                double ratio = 1.0;
                Guid[] ibs = guids.ToArray();

                if (ibs != null && ibs.Length > 0)
                {
                    AABB bb0 = new AABB();
                    if (ibs != null)
                        foreach (Guid guid in ibs)
                        {
                            if (guid != Guid.Empty)
                            {
                                InstanceAABBModel ib = AppWorkspace.Instance.InstanceManager.getAABBModel(guid);
                                if (ib == null)
                                    continue;
                                bb0.update(ib.AABBMINX, ib.AABBMINY, ib.AABBMINZ);
                                bb0.update(ib.AABBMAXX, ib.AABBMAXY, ib.AABBMAXZ);
                            }
                        }

                    if (bb0.Valid)
                    {
                        bb = bb0;
                        ratio = 1.5f;
                        //내가 만든 Class에 ViewController로 바꿔줌.
                        _sceneViewController.perspectiveFitViewAnimation(_sceneViewController.Camera.getViewVector(), bb, (float)ratio);
                        
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                TryCatchContext.Instance.doCatch(ex);
            }
        }
        private InstanceGroupOC _instanceGroupOC;
        private string UNDEFINED_GROUP_NAME;

        //UcInstanceGroups
        public void CreateTree()
        {
            Guid currentID = AppWorkspace.Instance.CurrentMode.CurrentID;
            bool hasRoot = false;
            //refineGroupData();
            ObservableCollection<InstanceGroupVM> bindingList = new ObservableCollection<InstanceGroupVM>(_instanceGroupOC.ViewModels.Distinct().ToList().FindAll(x => x.BUILDING_ID == currentID).OrderBy(x => x.GROUP_NM));
            foreach (InstanceGroupVM vm in bindingList)
            {
                if (vm.PARENT_GROUP_ID.Equals(Guid.Empty))
                {
                    hasRoot = true;
                    break;
                }
                vm.IsChecked = true;
            }
            if (!hasRoot)
            {
                CurrentViewModeType viewMode = AppWorkspace.Instance.CurrentMode.CurrentModeType;
                if (viewMode.Equals(CurrentViewModeType.Site))
                {
                    InstanceGroupDataModel siteModel = _instanceGroupOC.newDataModel();
                    siteModel.ID = currentID;
                    siteModel.PARENT_ID = Guid.Empty;
                    siteModel.BUILDING_ID = currentID;
                    if (AppWorkspace.Instance.SiteManager.CurrentSite.SiteName == null)
                        siteModel.NAME = "";
                    else
                        siteModel.NAME = AppWorkspace.Instance.SiteManager.CurrentSite.SiteName;
                    siteModel.GROUP_TYPE = 5000;
                    InstanceGroupVM vm = new InstanceGroupVM(siteModel);
                    if (!vm.GROUP_NM.Equals(""))
                        vm.updateData();
                    bindingList.Add(vm);
                }
                else if (viewMode.Equals(CurrentViewModeType.Building))
                {
                    InstanceGroupDataModel buildingModel = _instanceGroupOC.newDataModel();
                    buildingModel.ID = currentID;
                    buildingModel.PARENT_ID = Guid.Empty;
                    buildingModel.BUILDING_ID = currentID;
                    if (AppWorkspace.Instance.SiteManager.CurrentSite.CurrentBuilding == null || AppWorkspace.Instance.SiteManager.CurrentSite.CurrentBuilding.BuildingName == null)
                        buildingModel.NAME = "";
                    else
                        buildingModel.NAME = AppWorkspace.Instance.SiteManager.CurrentSite.CurrentBuilding.BuildingName;
                    buildingModel.GROUP_TYPE = 5000;
                    InstanceGroupVM vm = new InstanceGroupVM(buildingModel);
                    if (!vm.GROUP_NM.Equals(""))
                        vm.updateData();
                    bindingList.Add(vm);
                }
            }

            //groupTree.ItemsSource = bindingList;
            //groupTreeView.CheckAllNodes();

            //if (groupTreeView.Nodes.Count > 0)
            //{
            //    groupTreeView.Nodes[0].IsExpanded = true;

            //    this._nodeCheckedState.Clear();
            //    this._nodeExpandedState.Clear();
            //    this.refreshNodeState(groupTreeView.Nodes[0]);
            //}

            //groupTreeView.NodeCheckStateChanged += groupTreeView_NodeCheckStateChanged;
            //groupTreeView.NodeExpanded += GroupTreeView_NodeExpanded;
            //groupTreeView.NodeCollapsed += GroupTreeView_NodeExpanded;
            //groupTreeView.ToolTipOpening += GroupTreeView_ToolTipOpening;
            //groupTreeView.ToolTip = "f";
        }
    
        private AppWorkspace openWorkspace(ProjectBase project)
        {
            AppWorkspace instance = null;

            try
            {
                AppWorkspace.releaseInstance();

                if (File.Exists(project.DBFileName))
                    AppWorkspace.newInstance(project);
                else
                    return null;

                if (!AppWorkspace.Instance.Open(project.DBFileName))
                    AppWorkspace.releaseInstance();

                instance = AppWorkspace.Instance;
                if (instance == null)
                    throw new Exception("프로젝트 생성 중 문제가 발생하였습니다. 라이센스를 확인해주세요.");
            }
            catch (ProtocolException pe)
            {
                MessageBox.Show(pe.Message);
            }
            catch (Exception ex)
            {
                AppWorkspace.releaseInstance();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                TryCatchContext.Instance.FilePath = System.IO.Path.Combine(project.Directory, "error.log");
            }

            return instance;
        }
    }
}   