using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Comparison;
using ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Models;

namespace ZumtenSoft.Linq2ObsCollection.Samples.FolderSize
{
    /// <summary>
    /// Interaction logic for FolderSizeControl.xaml
    /// </summary>
    public partial class FolderSizeControl
    {
        public FolderViewModel Root { get; private set; }
        public ProcessingContext Context { get; private set; }
        private readonly SortingObservatorCollection<FileViewModel> _filesSource;

        public FolderSizeControl()
        {
            InitializeComponent();
            TreeFolders.SelectedItemChanged += TreeFoldersOnSelectedItemChanged;

            List<SortingOption<FolderViewModel>> foldersSortingOptions = new List<SortingOption<FolderViewModel>>();
            foldersSortingOptions.Add(new SortingOption<FolderViewModel>("Size", ComparerBuilder<FolderViewModel>.OrderByDescending(x => x.Length).Comparer));
            foldersSortingOptions.Add(new SortingOption<FolderViewModel>("Name", ComparerBuilder<FolderViewModel>.OrderBy(x => x.Name).Comparer));

            List<SortingOption<FileViewModel>> filesSortingOptions = new List<SortingOption<FileViewModel>>();
            filesSortingOptions.Add(new SortingOption<FileViewModel>("Size", ComparerBuilder<FileViewModel>.OrderByDescending(x => x.Length).Comparer));
            filesSortingOptions.Add(new SortingOption<FileViewModel>("Name", ComparerBuilder<FileViewModel>.OrderBy(x => x.Name).Comparer));

            Context = new ProcessingContext(Dispatcher, 3, foldersSortingOptions[0].Comparer);
            Context.SizeProcessingQueue.Completed += SizeProcessingQueue_Completed;
            _filesSource = new SortingObservatorCollection<FileViewModel>(null, filesSortingOptions[0].Comparer);

            LstSortingFolders.ItemsSource = foldersSortingOptions;
            LstSortingFolders.SelectedIndex = 0;
            LstSortingFiles.ItemsSource = filesSortingOptions;
            LstSortingFiles.SelectedIndex = 0;

            DetailsView.ItemsSource = _filesSource;
        }

        private void TreeFoldersOnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FolderViewModel newValue = e.NewValue as FolderViewModel;
            _filesSource.Source = newValue == null ? null : newValue.Files;
        }

        private void Button_Browse(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = TxtFolderPath.Text;

            DialogResult result = folderDialog.ShowDialog();
            if (result.ToString() == "OK")
                TxtFolderPath.Text = folderDialog.SelectedPath;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!Context.IsRunning)
            {
                Context.IsRunning = true;
                BtnStart.Content = "Stop";

                DirectoryInfo root = new DirectoryInfo(TxtFolderPath.Text);
                Folder rootFolder = new Folder(null, root, Context.SizeProcessingQueue);
                Root = new FolderViewModel(null, rootFolder, Context);
                TreeFolders.ItemsSource = Root.SubFolders;
            }
            else
            {
                Context.SizeProcessingQueue.Clear();
            }
        }

        private void SizeProcessingQueue_Completed(object sender, System.EventArgs e)
        {
            Context.UpdateSizeDispatcher.Push(() =>
            {
                Context.IsRunning = false;
                BtnStart.Content = "Start";
            });
        }

        private void lstSortingFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.FolderComparer = ((SortingOption<FolderViewModel>)LstSortingFolders.SelectedItem).Comparer;
            if (Root != null)
                Root.UpdateSorting();
        }

        private void lstSortingFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _filesSource.Comparer = ((SortingOption<FileViewModel>) LstSortingFiles.SelectedItem).Comparer;
        }
    }
}
