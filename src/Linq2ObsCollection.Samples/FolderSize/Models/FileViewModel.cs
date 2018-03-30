using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Icons;

namespace ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Models
{
    public class FileViewModel : NotifyObject
    {
        private readonly ProcessingContext _context;

        public FileViewModel(FileInfo source, ProcessingContext context)
        {
            _context = context;
            Source = source;
            Length = Source.Length;
            Name = Source.Name;
        }

        public FileInfo Source { get; private set; }
        public string Name { get; private set; }
        public long Length { get; private set; }

        private BitmapSource _image;
        public BitmapSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = IconsCollection.Waiting;

                    _context.IconProcessingQueue.Push(() =>
                    {
                        using (Icon icon = Icon.FromHandle(Icon.ExtractAssociatedIcon(Source.FullName).ToBitmap().GetHicon()))
                        {
                            var newImage =Imaging.CreateBitmapSourceFromHIcon(
                                                    icon.Handle,
                                                    new Int32Rect(0, 0, icon.Width, icon.Height),
                                                    BitmapSizeOptions.FromEmptyOptions());
                            newImage.Freeze();
                            _image = newImage;
                        }

                        _context.UpdateSizeDispatcher.Push(() => Notify("Image"));
                    });
                }
                return _image;
            }
        }
    }
}