﻿#pragma checksum "C:\Users\Gab\Documents\DEV\gsc-field-application\GSCFieldApp\Views\Shell.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "829560853BEF1B4330FA9E31D01C06B6"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GSCFieldApp.Views
{
    partial class Shell : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Template10_Controls_HamburgerButtonInfo_IsEnabled(global::Template10.Controls.HamburgerButtonInfo obj, global::System.Boolean value)
            {
                obj.IsEnabled = value;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class Shell_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IShell_Bindings
        {
            private global::GSCFieldApp.Views.Shell dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Template10.Controls.HamburgerButtonInfo obj5;
            private global::Template10.Controls.HamburgerButtonInfo obj6;
            private global::Template10.Controls.HamburgerButtonInfo obj8;

            // Fields for each event bindings event handler.
            private global::Windows.UI.Xaml.RoutedEventHandler obj8Tapped;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj5IsEnabledDisabled = false;
            private static bool isobj6IsEnabledDisabled = false;
            private static bool isobj8IsEnabledDisabled = false;

            private Shell_obj1_BindingsTracking bindingsTracking;

            public Shell_obj1_Bindings()
            {
                this.bindingsTracking = new Shell_obj1_BindingsTracking(this);
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 25 && columnNumber == 92)
                {
                    isobj5IsEnabledDisabled = true;
                }
                else if (lineNumber == 34 && columnNumber == 122)
                {
                    isobj6IsEnabledDisabled = true;
                }
                else if (lineNumber == 50 && columnNumber == 151)
                {
                    isobj8IsEnabledDisabled = true;
                }
                else if (lineNumber == 50 && columnNumber == 92)
                {
                    this.obj8.Tapped -= obj8Tapped;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 5: // Views\Shell.xaml line 25
                        this.obj5 = (global::Template10.Controls.HamburgerButtonInfo)target;
                        this.bindingsTracking.RegisterTwoWayListener_5(this.obj5);
                        break;
                    case 6: // Views\Shell.xaml line 34
                        this.obj6 = (global::Template10.Controls.HamburgerButtonInfo)target;
                        this.bindingsTracking.RegisterTwoWayListener_6(this.obj6);
                        break;
                    case 8: // Views\Shell.xaml line 50
                        this.obj8 = (global::Template10.Controls.HamburgerButtonInfo)target;
                        this.obj8Tapped = (global::System.Object p0, global::Windows.UI.Xaml.RoutedEventArgs p1) =>
                        {
                            this.dataRoot.SViewModel.QuickBackupAsync();
                        };
                        ((global::Template10.Controls.HamburgerButtonInfo)target).Tapped += obj8Tapped;
                        this.bindingsTracking.RegisterTwoWayListener_8(this.obj8);
                        break;
                    default:
                        break;
                }
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                throw new global::System.NotImplementedException();
            }

            public void Recycle()
            {
                throw new global::System.NotImplementedException();
            }

            // IShell_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
                this.bindingsTracking.ReleaseAllListeners();
                this.initialized = false;
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                this.bindingsTracking.ReleaseAllListeners();
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::GSCFieldApp.Views.Shell)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::GSCFieldApp.Views.Shell obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_SViewModel(obj.SViewModel, phase);
                    }
                }
            }
            private void Update_SViewModel(global::GSCFieldApp.ViewModels.ShellViewModel obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_SViewModel(obj);
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_SViewModel_ShellEnableMapCommand(obj.ShellEnableMapCommand, phase);
                        this.Update_SViewModel_ShellEnableNoteCommand(obj.ShellEnableNoteCommand, phase);
                        this.Update_SViewModel_ShellEnableBackupCommand(obj.ShellEnableBackupCommand, phase);
                    }
                }
            }
            private void Update_SViewModel_ShellEnableMapCommand(global::System.Boolean obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // Views\Shell.xaml line 25
                    if (!isobj5IsEnabledDisabled)
                    {
                        XamlBindingSetters.Set_Template10_Controls_HamburgerButtonInfo_IsEnabled(this.obj5, obj);
                    }
                }
            }
            private void Update_SViewModel_ShellEnableNoteCommand(global::System.Boolean obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // Views\Shell.xaml line 34
                    if (!isobj6IsEnabledDisabled)
                    {
                        XamlBindingSetters.Set_Template10_Controls_HamburgerButtonInfo_IsEnabled(this.obj6, obj);
                    }
                }
            }
            private void Update_SViewModel_ShellEnableBackupCommand(global::System.Boolean obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // Views\Shell.xaml line 50
                    if (!isobj8IsEnabledDisabled)
                    {
                        XamlBindingSetters.Set_Template10_Controls_HamburgerButtonInfo_IsEnabled(this.obj8, obj);
                    }
                }
            }
            private void UpdateTwoWay_5_IsEnabled()
            {
                if (this.initialized)
                {
                    if (this.dataRoot != null)
                    {
                        if (this.dataRoot.SViewModel != null)
                        {
                            this.dataRoot.SViewModel.ShellEnableMapCommand = this.obj5.IsEnabled;
                        }
                    }
                }
            }
            private void UpdateTwoWay_6_IsEnabled()
            {
                if (this.initialized)
                {
                    if (this.dataRoot != null)
                    {
                        if (this.dataRoot.SViewModel != null)
                        {
                            this.dataRoot.SViewModel.ShellEnableNoteCommand = this.obj6.IsEnabled;
                        }
                    }
                }
            }
            private void UpdateTwoWay_8_IsEnabled()
            {
                if (this.initialized)
                {
                    if (this.dataRoot != null)
                    {
                        if (this.dataRoot.SViewModel != null)
                        {
                            this.dataRoot.SViewModel.ShellEnableBackupCommand = this.obj8.IsEnabled;
                        }
                    }
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class Shell_obj1_BindingsTracking
            {
                private global::System.WeakReference<Shell_obj1_Bindings> weakRefToBindingObj; 

                public Shell_obj1_BindingsTracking(Shell_obj1_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<Shell_obj1_Bindings>(obj);
                }

                public Shell_obj1_Bindings TryGetBindingObject()
                {
                    Shell_obj1_Bindings bindingObject = null;
                    if (weakRefToBindingObj != null)
                    {
                        weakRefToBindingObj.TryGetTarget(out bindingObject);
                        if (bindingObject == null)
                        {
                            weakRefToBindingObj = null;
                            ReleaseAllListeners();
                        }
                    }
                    return bindingObject;
                }

                public void ReleaseAllListeners()
                {
                    UpdateChildListeners_SViewModel(null);
                }

                public void PropertyChanged_SViewModel(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
                {
                    Shell_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        string propName = e.PropertyName;
                        global::GSCFieldApp.ViewModels.ShellViewModel obj = sender as global::GSCFieldApp.ViewModels.ShellViewModel;
                        if (global::System.String.IsNullOrEmpty(propName))
                        {
                            if (obj != null)
                            {
                                bindings.Update_SViewModel_ShellEnableMapCommand(obj.ShellEnableMapCommand, DATA_CHANGED);
                                bindings.Update_SViewModel_ShellEnableNoteCommand(obj.ShellEnableNoteCommand, DATA_CHANGED);
                                bindings.Update_SViewModel_ShellEnableBackupCommand(obj.ShellEnableBackupCommand, DATA_CHANGED);
                            }
                        }
                        else
                        {
                            switch (propName)
                            {
                                case "ShellEnableMapCommand":
                                {
                                    if (obj != null)
                                    {
                                        bindings.Update_SViewModel_ShellEnableMapCommand(obj.ShellEnableMapCommand, DATA_CHANGED);
                                    }
                                    break;
                                }
                                case "ShellEnableNoteCommand":
                                {
                                    if (obj != null)
                                    {
                                        bindings.Update_SViewModel_ShellEnableNoteCommand(obj.ShellEnableNoteCommand, DATA_CHANGED);
                                    }
                                    break;
                                }
                                case "ShellEnableBackupCommand":
                                {
                                    if (obj != null)
                                    {
                                        bindings.Update_SViewModel_ShellEnableBackupCommand(obj.ShellEnableBackupCommand, DATA_CHANGED);
                                    }
                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                    }
                }
                private global::GSCFieldApp.ViewModels.ShellViewModel cache_SViewModel = null;
                public void UpdateChildListeners_SViewModel(global::GSCFieldApp.ViewModels.ShellViewModel obj)
                {
                    if (obj != cache_SViewModel)
                    {
                        if (cache_SViewModel != null)
                        {
                            ((global::System.ComponentModel.INotifyPropertyChanged)cache_SViewModel).PropertyChanged -= PropertyChanged_SViewModel;
                            cache_SViewModel = null;
                        }
                        if (obj != null)
                        {
                            cache_SViewModel = obj;
                            ((global::System.ComponentModel.INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged_SViewModel;
                        }
                    }
                }
                public void RegisterTwoWayListener_5(global::Template10.Controls.HamburgerButtonInfo sourceObject)
                {
                    sourceObject.RegisterPropertyChangedCallback(global::Template10.Controls.HamburgerButtonInfo.IsEnabledProperty, (sender, prop) =>
                    {
                        var bindingObj = this.TryGetBindingObject();
                        if (bindingObj != null)
                        {
                            bindingObj.UpdateTwoWay_5_IsEnabled();
                        }
                    });
                }
                public void RegisterTwoWayListener_6(global::Template10.Controls.HamburgerButtonInfo sourceObject)
                {
                    sourceObject.RegisterPropertyChangedCallback(global::Template10.Controls.HamburgerButtonInfo.IsEnabledProperty, (sender, prop) =>
                    {
                        var bindingObj = this.TryGetBindingObject();
                        if (bindingObj != null)
                        {
                            bindingObj.UpdateTwoWay_6_IsEnabled();
                        }
                    });
                }
                public void RegisterTwoWayListener_8(global::Template10.Controls.HamburgerButtonInfo sourceObject)
                {
                    sourceObject.RegisterPropertyChangedCallback(global::Template10.Controls.HamburgerButtonInfo.IsEnabledProperty, (sender, prop) =>
                    {
                        var bindingObj = this.TryGetBindingObject();
                        if (bindingObj != null)
                        {
                            bindingObj.UpdateTwoWay_8_IsEnabled();
                        }
                    });
                }
            }
        }
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // Views\Shell.xaml line 16
                {
                    this.RootGrid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 3: // Views\Shell.xaml line 20
                {
                    this.MyHamburgerMenu = (global::Template10.Controls.HamburgerMenu)(target);
                }
                break;
            case 4: // Views\Shell.xaml line 91
                {
                    this.ShellProgressRingShell = (global::Windows.UI.Xaml.Controls.ProgressRing)(target);
                }
                break;
            case 6: // Views\Shell.xaml line 34
                {
                    this.ButtonReportPage = (global::Template10.Controls.HamburgerButtonInfo)(target);
                }
                break;
            case 7: // Views\Shell.xaml line 36
                {
                    this.ButtonReportPageIcon = (global::Windows.UI.Xaml.Controls.SymbolIcon)(target);
                }
                break;
            case 8: // Views\Shell.xaml line 50
                {
                    this.ButtonDBBackup = (global::Template10.Controls.HamburgerButtonInfo)(target);
                }
                break;
            case 9: // Views\Shell.xaml line 60
                {
                    this.ProjectsButton = (global::Template10.Controls.HamburgerButtonInfo)(target);
                }
                break;
            case 10: // Views\Shell.xaml line 75
                {
                    this.SettingsButton = (global::Template10.Controls.HamburgerButtonInfo)(target);
                }
                break;
            case 11: // Views\Shell.xaml line 52
                {
                    this.ButtonBackupDB = (global::Windows.UI.Xaml.Controls.SymbolIcon)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            switch(connectionId)
            {
            case 1: // Views\Shell.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    Shell_obj1_Bindings bindings = new Shell_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element1, bindings);
                }
                break;
            }
            return returnValue;
        }
    }
}

