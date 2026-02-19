namespace MmLogView.Properties {
    using System;
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MmLogView.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }

        public static string BtnCancel => ResourceManager.GetString("BtnCancel", resourceCulture)!;
        public static string BtnGoTo => ResourceManager.GetString("BtnGoTo", resourceCulture)!;
        public static string BtnOk => ResourceManager.GetString("BtnOk", resourceCulture)!;
        public static string BtnOpen => ResourceManager.GetString("BtnOpen", resourceCulture)!;
        public static string BtnSearch => ResourceManager.GetString("BtnSearch", resourceCulture)!;
        public static string FeatureText => ResourceManager.GetString("FeatureText", resourceCulture)!;
        public static string GoToLineLabel => ResourceManager.GetString("GoToLineLabel", resourceCulture)!;
        public static string GoToLineTitle => ResourceManager.GetString("GoToLineTitle", resourceCulture)!;
        public static string InvalidInputTitle => ResourceManager.GetString("InvalidInputTitle", resourceCulture)!;
        public static string InvalidLineInput => ResourceManager.GetString("InvalidLineInput", resourceCulture)!;
        public static string LineDone => ResourceManager.GetString("LineDone", resourceCulture)!;
        public static string LineScanning => ResourceManager.GetString("LineScanning", resourceCulture)!;
        public static string MenuCopyNode => ResourceManager.GetString("MenuCopyNode", resourceCulture)!;
        public static string MenuCopyNodeAndChildren => ResourceManager.GetString("MenuCopyNodeAndChildren", resourceCulture)!;
        public static string MenuCopyPage => ResourceManager.GetString("MenuCopyPage", resourceCulture)!;
        public static string MenuCopySelected => ResourceManager.GetString("MenuCopySelected", resourceCulture)!;
        public static string MenuOpenLineNotepad => ResourceManager.GetString("MenuOpenLineNotepad", resourceCulture)!;
        public static string MenuOpenPageNotepad => ResourceManager.GetString("MenuOpenPageNotepad", resourceCulture)!;
        public static string OpenDialogFilter => ResourceManager.GetString("OpenDialogFilter", resourceCulture)!;
        public static string OpenDialogTitle => ResourceManager.GetString("OpenDialogTitle", resourceCulture)!;
        public static string OpenFailed => ResourceManager.GetString("OpenFailed", resourceCulture)!;
        public static string ReadyStatus => ResourceManager.GetString("ReadyStatus", resourceCulture)!;
        public static string SearchFoundAt => ResourceManager.GetString("SearchFoundAt", resourceCulture)!;
        public static string SearchNotFound => ResourceManager.GetString("SearchNotFound", resourceCulture)!;
        public static string ThemeTooltip => ResourceManager.GetString("ThemeTooltip", resourceCulture)!;
    }
}
