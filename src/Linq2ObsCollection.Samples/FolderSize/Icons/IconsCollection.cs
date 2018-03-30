using System;
using System.Windows.Media.Imaging;

namespace ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Icons
{
    public static class IconsCollection
    {
        private static BitmapImage Build(string name)
        {
            return new BitmapImage(new Uri("pack://application:,,,/FolderSize/Icons/" + name));
        }

        public static readonly BitmapImage
            FolderClosed = Build("FolderClosed.png"),
            FolderOpened = Build("FolderOpened.png"),
            Waiting = Build("Waiting.gif");
    }
}
