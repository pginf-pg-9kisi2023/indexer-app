using System.Windows.Input;

namespace Indexer.View
{
    internal class Commands
    {
        public static RoutedUICommand CreateNewSession = new RoutedUICommand(
            "Create New Session", "CreateNewSession", typeof(Commands)
        );

        public static RoutedUICommand LoadSession = new RoutedUICommand(
            "Load Session", "LoadSession", typeof(Commands)
        );

        public static RoutedUICommand AddImageOrFolder = new RoutedUICommand(
            "Open Image Or Folder", "OpenImageOrFolder", typeof(Commands)
        );

        public static RoutedUICommand SaveSession = new RoutedUICommand(
            "Save Session", "SaveSession", typeof(Commands)
        );

        public static RoutedUICommand SaveSessionAs = new RoutedUICommand(
            "Save Session As", "SaveSessionAs", typeof(Commands)
        );

        public static RoutedUICommand ExportTo = new RoutedUICommand(
            "Export To", "ExportTo", typeof(Commands)
        );

        public static RoutedUICommand ExportAsCSV = new RoutedUICommand(
            "Export As CSV", "ExportAsCSV", typeof(Commands)
        );

        public static RoutedUICommand ExportAsXML = new RoutedUICommand(
            "Export As XML", "ExportAsXML", typeof(Commands)
        );

        public static RoutedUICommand CloseSession = new RoutedUICommand(
            "Close Session", "CloseSession", typeof(Commands)
        );

        public static RoutedUICommand Exit = new RoutedUICommand(
            "Exit", "Exit", typeof(Commands)
        );

        public static RoutedUICommand ShortHelp = new RoutedUICommand(
            "Short Help", "ShortHelp", typeof(Commands)
        );

        public static RoutedUICommand AboutIndexer = new RoutedUICommand(
            "About Indexer", "AboutIndexer", typeof(Commands)
        );

        public static RoutedUICommand ChangeToNextImage = new RoutedUICommand(
            "Change To Next Image", "ChangeToNextImage", typeof(Commands)
        );

        public static RoutedUICommand ChangeToPreviousImage = new RoutedUICommand(
            "Change To Previous Image", "ChangeToPreviousImage", typeof(Commands)
        );

        public static RoutedUICommand AnalyzeImages = new RoutedUICommand(
            "Analyze Images", "AnalyzeImages", typeof(Commands)
        );
    }
}
