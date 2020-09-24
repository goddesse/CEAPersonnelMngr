using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.DirectoryServices.AccountManagement;
using System.Security;
using System.ComponentModel;
using System.Web;

namespace CEA_Personnel_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                lstAdmins.ItemsSource = CEARoles.CEAAdministrators.Users;
                lstGAs.ItemsSource = CEARoles.CEAGraduateAssistants.Users;
                lstTAs.ItemsSource = CEARoles.CEATechnicalAssistants.Users;
            }
            catch(TypeInitializationException)
            {
                MessageBox.Show("The AD server is down. Please check your network connection.");
                Application.Current.Shutdown(-1);
                return;
            }
            catch(UnauthorizedAccessException)
            {
                MessageBox.Show("This user doesn't have permission to modify CEA personnel. Please contact CEA technical support" +
                    " if you believe this message is in error.");
                Application.Current.Shutdown(-1);
                return;
            }
            catch(Exception e)
            {
                System.Diagnostics.Process.Start("mailto:ada@uark.edu?subject=CEA Personnel Manager Exception&body=" + HttpUtility.HtmlEncode(e.ToString()));
                Application.Current.Shutdown(-1);
                return;
            }

            CollectionView adminView = (CollectionView)CollectionViewSource.GetDefaultView(lstAdmins.ItemsSource);
            adminView.SortDescriptions.Add(new SortDescription("Surname", ListSortDirection.Ascending));

            CollectionView gaView = (CollectionView)CollectionViewSource.GetDefaultView(lstGAs.ItemsSource);
            gaView.SortDescriptions.Add(new SortDescription("Surname", ListSortDirection.Ascending));

            CollectionView taView = (CollectionView)CollectionViewSource.GetDefaultView(lstTAs.ItemsSource);
            taView.SortDescriptions.Add(new SortDescription("Surname", ListSortDirection.Ascending));
        }

        //Full credits for easy triangle Geometry from http://www.wpf-tutorial.com
        public class TriangleSortAdorner : Adorner
        {
            public TriangleSortAdorner(UIElement element, ListSortDirection dir) : base(element)
            {
                this.Direction = dir;
            }

            public ListSortDirection Direction { get; private set; }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if(AdornedElement.RenderSize.Width < 20)
                    return;

                var transformWidth = AdornedElement.RenderSize.Width - 15;
                var transformHeight = (AdornedElement.RenderSize.Height - 5) / 2;
                TranslateTransform transform = new TranslateTransform(transformWidth, transformHeight);

                drawingContext.PushTransform(transform);

                Geometry geometry =
                    (Direction == ListSortDirection.Ascending) ?
                    ascendingGeometry :
                    descendingGeometry;

                drawingContext.DrawGeometry(Brushes.Black, null, geometry);

                drawingContext.Pop();
            }

            private static Geometry ascendingGeometry = Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");
            private static Geometry descendingGeometry = Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            TabItem selTabItem = (TabItem)tbcUserList.SelectedItem;
            string headerName = selTabItem.Header.ToString();

            switch(headerName)
            {
                case "Administrators":
                    if(CEARoles.CEAAdministrators.AddUser(txtAdd.Text))
                    { MessageBox.Show("Successfully added " + txtAdd.Text + " as an administrator.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                    else
                    { MessageBox.Show("Could not add " + txtAdd.Text + ". Please check the username.", null, MessageBoxButton.OK, MessageBoxImage.Error); }
                    break;
                case "Graduate Assistants":
                    if(CEARoles.CEAGraduateAssistants.AddUser(txtAdd.Text))
                    { MessageBox.Show("Successfully added " + txtAdd.Text + " as a graduate assistant.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                    else
                    { MessageBox.Show("Could not add " + txtAdd.Text + ". Please check the username.", null, MessageBoxButton.OK, MessageBoxImage.Error); }
                    break;
                case "Student Workers":
                    if(CEARoles.CEATechnicalAssistants.AddUser(txtAdd.Text))
                    { MessageBox.Show("Successfully added " + txtAdd.Text + " as a student worker.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                    else
                    { MessageBox.Show("Could not add " + txtAdd.Text + ". Please check the username.", null, MessageBoxButton.OK, MessageBoxImage.Error); }
                    break;
                default:
                    MessageBox.Show("Could not add the user. Please contact CEA technical support.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            txtAdd.Text = "";
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            TabItem selTabItem = (TabItem)tbcUserList.SelectedItem;
            string headerName = selTabItem.Header.ToString();
            if(curUser == null) { MessageBox.Show("No user is selected. Please select a user to remove them from the group.", null, MessageBoxButton.OK, MessageBoxImage.Error); return; }

            switch(headerName)
            {
                case "Administrators":
                    if(CEARoles.CEAAdministrators.RemoveUser(curUser.Username))
                    { MessageBox.Show("Successfully removed " + curUser.Username + " as an administrator.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                    else
                    { MessageBox.Show("Could not remove " + curUser.Username + ". Please check the username.", null, MessageBoxButton.OK, MessageBoxImage.Error); }
                    break;
                case "Graduate Assistants":
                    if(CEARoles.CEAGraduateAssistants.RemoveUser(curUser.Username))
                    { MessageBox.Show("Successfully removed " + curUser.Username + " as a graduate assistant.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                    else
                    { MessageBox.Show("Could not remove " + curUser.Username + ". Please check the username.", null, MessageBoxButton.OK, MessageBoxImage.Error); }
                    break;
                case "Student Workers":
                    if(CEARoles.CEATechnicalAssistants.RemoveUser(curUser.Username))
                    { MessageBox.Show("Successfully removed " + curUser.Username + " as a student worker.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }
                    else
                    { MessageBox.Show("Could not remove " + curUser.Username + ". Please check the username.", null, MessageBoxButton.OK, MessageBoxImage.Error); }
                    break;
                default:
                    MessageBox.Show("Could not remove the user. Please contact CEA technical support.", null, MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            btnRemove.Content = "Remove";
        }

        private void lstAdmins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstAdmins.SelectedIndex > -1)
            {
                curUser = (CEARole.CEAUser)lstAdmins.SelectedItem;
                btnRemove.Content = "Remove " + curUser.Username;
            }
            else
            {
                btnRemove.Content = "Remove";
            }
        }

        private void lstGAs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstGAs.SelectedIndex > -1)
            {
                curUser = (CEARole.CEAUser)lstGAs.SelectedItem;
                btnRemove.Content = "Remove " + curUser.Username;
            }
            else
            {
                btnRemove.Content = "Remove";
            }
        }

        private void lstTAs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstTAs.SelectedIndex > -1)
            {
                curUser = (CEARole.CEAUser)lstTAs.SelectedItem;
                btnRemove.Content = "Remove " + curUser.Username;
            }
            else
            {
                btnRemove.Content = "Remove";
            }
        }

        private void lstAdminsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            string sortBy = column.Tag.ToString();

            if(listViewSortColumn != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortColumn).Remove(listViewSortAdorner);
                lstAdmins.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if(listViewSortColumn == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortColumn = column;
            listViewSortAdorner = new TriangleSortAdorner(listViewSortColumn, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortColumn).Add(listViewSortAdorner);
            lstAdmins.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void lstGAsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            string sortBy = column.Tag.ToString();

            if(listViewSortColumn != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortColumn).Remove(listViewSortAdorner);
                lstGAs.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if(listViewSortColumn == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortColumn = column;
            listViewSortAdorner = new TriangleSortAdorner(listViewSortColumn, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortColumn).Add(listViewSortAdorner);
            lstGAs.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void lstTAsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            string sortBy = column.Tag.ToString();

            if(listViewSortColumn != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortColumn).Remove(listViewSortAdorner);
                lstTAs.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if(listViewSortColumn == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortColumn = column;
            listViewSortAdorner = new TriangleSortAdorner(listViewSortColumn, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortColumn).Add(listViewSortAdorner);
            lstTAs.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private GridViewColumnHeader listViewSortColumn = null;
        private TriangleSortAdorner listViewSortAdorner = null;

        //Keep track of the currently selected CEAUser object (for the Remove click) no matter what pane is selected
        public CEARole.CEAUser curUser = null;
    }
}
