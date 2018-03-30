using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ZumtenSoft.WpfUtils.Collections;
using ZumtenSoft.WpfUtils.Comparison;
using ZumtenSoft.WpfUtils.Samples.FolderSize.Models;
using UserControl = System.Windows.Controls.UserControl;

namespace ZumtenSoft.WpfUtils.Samples.FolderSize
{
    /// <summary>
    /// Interaction logic for FolderSizeControl.xaml
    /// </summary>
    public partial class FolderSizeControl : UserControl
    {
        public FolderViewModel Root { get; private set; }
        public ProcessingContext Context { get; private set; }
        private readonly SortingObservatorCollection<FileViewModel> _filesSource;

        public FolderSizeControl()
        {
            InitializeComponent();
            treeFolders.SelectedItemChanged += TreeFoldersOnSelectedItemChanged;

            List<SortingOption<FolderViewModel>> foldersSortingOptions = new List<SortingOption<FolderViewModel>>();
            foldersSortingOptions.Add(new SortingOption<FolderViewModel>("Size", ComparerBuilder<FolderViewModel>.OrderByDescending(x => x.Length).Comparer));
            foldersSortingOptions.Add(new SortingOption<FolderViewModel>("Name", ComparerBuilder<FolderViewModel>.OrderBy(x => x.Name).Comparer));

            List<SortingOption<FileViewModel>> filesSortingOptions = new List<SortingOption<FileViewModel>>();
            filesSortingOptions.Add(new SortingOption<FileViewModel>("Size", ComparerBuilder<FileViewModel>.OrderByDescending(x => x.Length).Comparer));
            filesSortingOptions.Add(new SortingOption<FileViewModel>("Name", ComparerBuilder<FileViewModel>.OrderBy(x => x.Name).Comparer));

            Context = new ProcessingContext(Dispatcher, 3, foldersSortingOptions[0].Comparer);
            Context.SizeProcessingQueue.Completed += SizeProcessingQueue_Completed;
            _filesSource = new SortingObservatorCollection<FileViewModel>(null, filesSortingOptions[0].Comparer);

            lstSortingFolders.ItemsSource = foldersSortingOptions;
            lstSortingFolders.SelectedIndex = 0;
            lstSortingFiles.ItemsSource = filesSortingOptions;
            lstSortingFiles.SelectedIndex = 0;

            detailsView.ItemsSource = _filesSource;
        }

        private void TreeFoldersOnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FolderViewModel newValue = e.NewValue as FolderViewModel;
            _filesSource.Source = newValue == null ? null : newValue.Files;
        }

        private void Button_Browse(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = txtFolderPath.Text;

            DialogResult result = folderDialog.ShowDialog();
            if (result.ToString() == "OK")
                txtFolderPath.Text = folderDialog.SelectedPath;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!Context.IsRunning)
            {
                Context.IsRunning = true;
                btnStart.Content = "Stop";

                DirectoryInfo root = new DirectoryInfo(txtFolderPath.Text);
                Folder rootFolder = new Folder(null, root, Context.SizeProcessingQueue);
                Root = new FolderViewModel(null, rootFolder, Context);
                treeFolders.ItemsSource = Root.SubFolders;
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
                btnStart.Content = "Start";
            });
        }

        private void lstSortingFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Context.FolderComparer = ((SortingOption<FolderViewModel>)lstSortingFolders.SelectedItem).Comparer;
            if (Root != null)
                Root.UpdateSorting();
        }

        private void lstSortingFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _filesSource.Comparer = ((SortingOption<FileViewModel>) lstSortingFiles.SelectedItem).Comparer;
        }
    }
}
