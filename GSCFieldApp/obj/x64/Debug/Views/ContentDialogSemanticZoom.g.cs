﻿#pragma checksum "C:\Users\Maison\Source\Repos\GSC-Field-Application\GSCFieldApp\Views\ContentDialogSemanticZoom.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "03E69347F8A6D5012352FDCA71B6BEF5"
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
    partial class ContentDialogSemanticZoom : 
        global::Windows.UI.Xaml.Controls.ContentDialog, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Windows_UI_Xaml_FrameworkElement_RequestedTheme(global::Windows.UI.Xaml.FrameworkElement obj, global::Windows.UI.Xaml.ElementTheme value)
            {
                obj.RequestedTheme = value;
            }
            public static void Set_Windows_UI_Xaml_Data_CollectionViewSource_Source(global::Windows.UI.Xaml.Data.CollectionViewSource obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.Source = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_Primitives_Selector_SelectedIndex(global::Windows.UI.Xaml.Controls.Primitives.Selector obj, global::System.Int32 value)
            {
                obj.SelectedIndex = value;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class ContentDialogSemanticZoom_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IContentDialogSemanticZoom_Bindings
        {
            private global::GSCFieldApp.Views.ContentDialogSemanticZoom dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::System.WeakReference obj1;
            private global::Windows.UI.Xaml.Data.CollectionViewSource obj2;
            private global::Windows.UI.Xaml.Controls.ListView obj6;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj1RequestedThemeDisabled = false;
            private static bool isobj2SourceDisabled = false;
            private static bool isobj6SelectedIndexDisabled = false;

            private ContentDialogSemanticZoom_obj1_BindingsTracking bindingsTracking;

            public ContentDialogSemanticZoom_obj1_Bindings()
            {
                this.bindingsTracking = new ContentDialogSemanticZoom_obj1_BindingsTracking(this);
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 17 && columnNumber == 5)
                {
                    isobj1RequestedThemeDisabled = true;
                }
                else if (lineNumber == 24 && columnNumber == 92)
                {
                    isobj2SourceDisabled = true;
                }
                else if (lineNumber == 56 && columnNumber == 191)
                {
                    isobj6SelectedIndexDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 1: // Views\ContentDialogSemanticZoom.xaml line 1
                        this.obj1 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.ContentDialog)target);
                        break;
                    case 2: // Views\ContentDialogSemanticZoom.xaml line 24
                        this.obj2 = (global::Windows.UI.Xaml.Data.CollectionViewSource)target;
                        break;
                    case 6: // Views\ContentDialogSemanticZoom.xaml line 56
                        this.obj6 = (global::Windows.UI.Xaml.Controls.ListView)target;
                        this.bindingsTracking.RegisterTwoWayListener_6(this.obj6);
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

            // IContentDialogSemanticZoom_Bindings

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
                    this.dataRoot = (global::GSCFieldApp.Views.ContentDialogSemanticZoom)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::GSCFieldApp.Views.ContentDialogSemanticZoom obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_ViewModel(obj.ViewModel, phase);
                    }
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update__selectedIndex(obj._selectedIndex, phase);
                    }
                }
            }
            private void Update_ViewModel(global::GSCFieldApp.ViewModels.ContentDialogSemanticZoomViewModel obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_ViewModel_userTheme(obj.userTheme, phase);
                        this.Update_ViewModel_Groups(obj.Groups, phase);
                    }
                }
            }
            private void Update_ViewModel_userTheme(global::Windows.UI.Xaml.ElementTheme obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // Views\ContentDialogSemanticZoom.xaml line 1
                    if (!isobj1RequestedThemeDisabled)
                    {
                        if ((this.obj1.Target as global::Windows.UI.Xaml.Controls.ContentDialog) != null)
                        {
                            XamlBindingSetters.Set_Windows_UI_Xaml_FrameworkElement_RequestedTheme((this.obj1.Target as global::Windows.UI.Xaml.Controls.ContentDialog), obj);
                        }
                    }
                }
            }
            private void Update_ViewModel_Groups(global::System.Collections.ObjectModel.ObservableCollection<global::GSCFieldApp.Models.SemanticDataGroup> obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // Views\ContentDialogSemanticZoom.xaml line 24
                    if (!isobj2SourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Data_CollectionViewSource_Source(this.obj2, obj, null);
                    }
                }
            }
            private void Update__selectedIndex(global::System.Int32 obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // Views\ContentDialogSemanticZoom.xaml line 56
                    if (!isobj6SelectedIndexDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_Primitives_Selector_SelectedIndex(this.obj6, obj);
                    }
                }
            }
            private void UpdateTwoWay_6_SelectedIndex()
            {
                if (this.initialized)
                {
                    if (this.dataRoot != null)
                    {
                        this.dataRoot._selectedIndex = this.obj6.SelectedIndex;
                    }
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class ContentDialogSemanticZoom_obj1_BindingsTracking
            {
                private global::System.WeakReference<ContentDialogSemanticZoom_obj1_Bindings> weakRefToBindingObj; 

                public ContentDialogSemanticZoom_obj1_BindingsTracking(ContentDialogSemanticZoom_obj1_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<ContentDialogSemanticZoom_obj1_Bindings>(obj);
                }

                public ContentDialogSemanticZoom_obj1_Bindings TryGetBindingObject()
                {
                    ContentDialogSemanticZoom_obj1_Bindings bindingObject = null;
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
                }

                public void RegisterTwoWayListener_6(global::Windows.UI.Xaml.Controls.ListView sourceObject)
                {
                    sourceObject.RegisterPropertyChangedCallback(global::Windows.UI.Xaml.Controls.Primitives.Selector.SelectedIndexProperty, (sender, prop) =>
                    {
                        var bindingObj = this.TryGetBindingObject();
                        if (bindingObj != null)
                        {
                            bindingObj.UpdateTwoWay_6_SelectedIndex();
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
            case 1: // Views\ContentDialogSemanticZoom.xaml line 1
                {
                    this.SemanticZoomContentDialog = (global::Windows.UI.Xaml.Controls.ContentDialog)(target);
                    ((global::Windows.UI.Xaml.Controls.ContentDialog)this.SemanticZoomContentDialog).PrimaryButtonClick += this.SemanticZoomContentDialog_PrimaryButtonClick;
                }
                break;
            case 2: // Views\ContentDialogSemanticZoom.xaml line 24
                {
                    this.Collection = (global::Windows.UI.Xaml.Data.CollectionViewSource)(target);
                }
                break;
            case 3: // Views\ContentDialogSemanticZoom.xaml line 31
                {
                    this.pageHeader = (global::Template10.Controls.PageHeader)(target);
                }
                break;
            case 4: // Views\ContentDialogSemanticZoom.xaml line 38
                {
                    this.semanticZoom = (global::Windows.UI.Xaml.Controls.SemanticZoom)(target);
                }
                break;
            case 6: // Views\ContentDialogSemanticZoom.xaml line 56
                {
                    this.semanticZoomListView = (global::Windows.UI.Xaml.Controls.ListView)(target);
                    ((global::Windows.UI.Xaml.Controls.ListView)this.semanticZoomListView).DoubleTapped += this.semanticZoomListView_DoubleTapped;
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
            case 1: // Views\ContentDialogSemanticZoom.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.ContentDialog element1 = (global::Windows.UI.Xaml.Controls.ContentDialog)target;
                    ContentDialogSemanticZoom_obj1_Bindings bindings = new ContentDialogSemanticZoom_obj1_Bindings();
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

