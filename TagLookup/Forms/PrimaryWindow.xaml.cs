// Author: Kamal McDermott
// Date late edited: 02/27/2014
// Third party libraries: 
//              * NewtonSoft.Json:
//              * TagLib-Sharp: 
//              * EPPlus:
// APIs used: 
//              * AcoustID: http://www.acoustid.org
//              * Cover Art Archive: http://www.coverartarchive.org


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
using System.Windows.Shapes;

// form specific
using System.IO;
using TagLookup.Classes.MediaFiles;
using TagLookup.Classes.MediaFiles.Processing;
// lookup specific
using TagLookup.Classes.Scanner;
using TagLookup.Classes.Scanner.JSON;

namespace TagLookup.Forms
{
    /// <summary>
    /// Interaction logic for PrimaryWindow.xaml
    /// </summary>
    public partial class PrimaryWindow : Window
    {

        public delegate void ButtonClickableEvent(object sender, EventArgs e);


        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".JPEG", ".BMP", ".GIF", ".PNG" };
        public static readonly List<string> MediaFileExtensions = new List<string> { ".MP3", ".OGG", ".FLAC", ".M4A", ".AVI", ".MP4", ".WAV", ".AIFF", ".TTA"  };

        // form ui control mappings
        Dictionary<string, string>      FolderToListBoxNameMapping          { get; set; }
        Dictionary<string, Grid>        FolderGridMapping                   { get; set; }
        Dictionary<string, ListBox>     FolderListboxMapping                { get; set; }
        Dictionary<string, string>      FileToGridNameMapping               { get; set; }
        Dictionary<string, TabItem>     FileTabItemMapping                  { get; set; }
        Dictionary<string, Grid>        FileGridMapping                     { get; set; }
        Dictionary<string, string>      PartialNameToMediaFileTabItemName   { get; set; }
        Dictionary<string, Grid>        MediaFileNameToMediaFileGrid        { get; set; }
        
        // track number of MediaFiles, used for MediaFile Grid naming, beginning with with 1 (for the first created MediaFile )
        public int NumberOfMediaFiles { get; set; }

        // for loading and parsing media files
        FileLoader FileLoader;
        MediaFileCollection mediaFileCollection { get; set; }
        List<Tuple<string, string>> ExceptionList { get; set; }
        
        // lookup class
        ScannedFiles ScannedFiles;
        public PrimaryWindow()
        {
            InitializeComponent( );
            mediaFileCollection = new MediaFileCollection( );
            FileLoader = new FileLoader();
            FolderToListBoxNameMapping = new Dictionary<string, string>( );
            FolderListboxMapping = new Dictionary<string, ListBox>( );
            FolderGridMapping = new Dictionary<string, Grid>( );
            ExceptionList = new List<Tuple<string, string>>( );
            FileToGridNameMapping = new Dictionary<string, string>( );
            FileTabItemMapping = new Dictionary<string, TabItem>( );
            FileGridMapping = new Dictionary<string, Grid>( );
            PartialNameToMediaFileTabItemName = new Dictionary<string, string>( );
            MediaFileNameToMediaFileGrid = new Dictionary<string, Grid>( );
            SetTabControlStyleToHideTabItemHeaders( tcFiles );
            SetTabControlStyleToHideTabItemHeaders( tcFileInformation );
            ScannedFiles = new ScannedFiles( );
        }
        // hides tabControl headers
        void SetTabControlStyleToHideTabItemHeaders(TabControl tc)
        {
            Style s = new Style( );
            s.Setters.Add( new Setter( UIElement.VisibilityProperty, Visibility.Collapsed ) );
            tc.ItemContainerStyle = s;
        }
        // unused
        private void btnFolder_Click( object sender, RoutedEventArgs e )
        {

        }
        
        // temporary for debugging
        private void DebugButton( object sender, RoutedEventArgs e )
        {
            var tcFileInfo_INFO = tcFileInformation;
            var scannedFilesInfo = ScannedFiles;
            var exceptions = ExceptionList;
        }
        
        // Add files and folders by dragging them onto the form 
        // for folders, will every file inside the folder and all of its subdirectories
        //  for valid media files
        private void Grid_Drop( object sender, DragEventArgs e )
        {
            bool isFile = e.Data.GetDataPresent(DataFormats.FileDrop);
            
            if(isFile)
            {
                    Task.Factory.StartNew( () =>
                    {
                        string [] fileNames = (string [])e.Data.GetData( DataFormats.FileDrop );
                        fileNames.ToList<string>( ).Sort( );
                        foreach ( string filename in fileNames )
                        {
                            if ( File.Exists( filename ) )
                            {
                                ProcessFile( filename );
                            }
                            else if ( Directory.Exists( filename ) )
                            {
                                string [] filesList = FileLoader.GetTemporaryFileNames( filename ).ToArray<string>( );
                                ProcessFiles( filesList );
                            }
                        }
                    } );
            }
        }
       
        // Populates the tcFiles TabControl with tabs for each folder with files scanned 
        Grid CreateFolderGridTabItem(MediaFile mf)
        {
            Grid grid = new Grid( );
            ListBox listBox = CreateFolderListBox( mf.ContainingFolder );
            listBox.FontSize = 9;
            grid.Name = SetGridName( mf.ContainingFolder, grid );

            grid.ColumnDefinitions.Add( new ColumnDefinition() );
            grid.ColumnDefinitions.Add( new ColumnDefinition( ) );

            Grid lbGrid = new Grid( );
            Grid artGrid = new Grid( );
            Grid.SetColumn( lbGrid, 0 );
            Grid.SetColumn( artGrid, 1 );


            // < col 1
            lbGrid.RowDefinitions.Add( new RowDefinition( ) );
            lbGrid.RowDefinitions.Add( new RowDefinition( ) );
            lbGrid.RowDefinitions [ 0 ].Height = new System.Windows.GridLength( 45 );
            Label lbl = new Label( );
            lbl.Content = mf.Album;
            Grid.SetRow( lbl, 0 );
            Grid.SetRow( listBox, 1 );
            lbGrid.Children.Add( lbl );
            lbGrid.Children.Add( listBox );
            // col 1 >


            // < col 2
            artGrid.RowDefinitions.Add( new RowDefinition( ) );
            artGrid.RowDefinitions.Add( new RowDefinition( ) );
            artGrid.RowDefinitions.Add( new RowDefinition( ) );

            artGrid.RowDefinitions [ 0 ].Height = new System.Windows.GridLength( 45 );
            artGrid.RowDefinitions [ 1 ].Height = new System.Windows.GridLength( 60 );
            
            listBox = new ListBox( );
            listBox.Name = "lbArtwork";
            DirectoryInfo di = new DirectoryInfo( mf.ContainingFolder );
            foreach(FileInfo fi in di.GetFiles() )
            {
                if( ImageExtensions.Contains(System.IO.Path.GetExtension(fi.FullName).ToUpperInvariant()))
                {
                    listBox.Items.Add( fi.FullName );
                }
            }
            if(Directory.Exists(di.Parent.FullName))
            {
                foreach ( FileInfo fi in di.Parent.GetFiles( ) )
                {
                    if ( ImageExtensions.Contains( System.IO.Path.GetExtension( fi.FullName ).ToUpperInvariant( ) ) )
                    {
                        listBox.Items.Add( fi.FullName );
                    }
                }
            }

            string imgPath = "";
            if ( listBox.Items.Count > 0 )
                imgPath = listBox.Items [ 0 ].ToString();

            Image img = new Image( );
            img.Source = CreateImageSource( imgPath );
            Grid.SetRow( img, 2 );
            artGrid.Children.Add( img );

            Grid.SetRow( listBox, 1 );
            
            artGrid.Children.Add( listBox );

            // col 2 >

            grid.Children.Add( lbGrid );
            grid.Children.Add( artGrid );
            return grid;
        }
     
        // 
        TabItem CreateFileTabItem( WriteableResult scannedFile, MediaFile mf, bool downloadArtwork )
        {
            TabItem tabItem = new TabItem( );
            tabItem.Name = SetTabItemName( mf.PartialFileName, tabItem );
            tabItem.Header = tabItem.Name;
            // Creates the primary Grid for the the tcFileInformation TabControl
            // Each column will contain a sub Grid
            // This Grid will named by the associated MediaFile's PartialFileName 
            // for mapping and selection purposes
            Grid primaryGrid = new Grid( );
            primaryGrid.Name = SetSubGridName( mf.PartialFileName, primaryGrid );
            
            primaryGrid.ColumnDefinitions.Add( new ColumnDefinition( ) );
            primaryGrid.ColumnDefinitions.Add( new ColumnDefinition( ) );

            Grid queryGrid = new Grid( );
            queryGrid.Name = "queryGrid";
            Grid.SetColumn( queryGrid, 0 );
        
            // queryGrid structure ---->
            queryGrid.RowDefinitions.Add( new RowDefinition( ) );
            queryGrid.RowDefinitions.Add( new RowDefinition( ) );
            // queryGrid structure ----!

            Grid gridWriteableResults = new Grid( );
            gridWriteableResults.RowDefinitions.Add( new RowDefinition( ) );
            gridWriteableResults.RowDefinitions.Add( new RowDefinition( ) );

            queryGrid.RowDefinitions [ 0 ].Height = new System.Windows.GridLength( 150 );

            TabControl tcResults = CreateWriteableResultTabControl( scannedFile, mf, downloadArtwork );
            Grid.SetRow( tcResults, 1 );
            queryGrid.Children.Add( tcResults );

            // mediafile specific grid, the second paramter is a form specific global to name the MediaFile specific grids
            Grid mfGrid = CreateMediaFileTabItem( mf, ++NumberOfMediaFiles );
            Grid.SetColumn( mfGrid, 1 );
            primaryGrid.Children.Add( mfGrid );

            // Add the constructed subgrids to the primaryGrid and then set the TabItem content to primaryGrid
            primaryGrid.Children.Add( queryGrid );
            
            tabItem.Content = primaryGrid;
            return tabItem;
        }
        
        Grid CreateMediaFileTabItem(MediaFile mf, int NumberOfMediaFiles )
        {
            Grid grid = new Grid( );
            grid.Name = "mfGrid" + NumberOfMediaFiles.ToString( );
            MediaFileNameToMediaFileGrid.Add( mf.FileName, grid );
            grid.RowDefinitions.Add( new RowDefinition( ) );
            grid.RowDefinitions.Add( new RowDefinition( ) );
            grid.RowDefinitions [ 0 ].Height = new System.Windows.GridLength( 150 ); ;
            Canvas canvas = new Canvas( );
            AddLabelAndTextBox( canvas, "lblTitle", "Title", 10, 70, "tbTitle", new string[]{ mf.Title }, 100, 70 );
            AddLabelAndTextBox( canvas, "lblArtist", "Artist", 10, 100, "tbArtist", new string [] { mf.Artist }, 100, 100 );
            AddLabelAndTextBox( canvas, "lblAlbum", "Album", 10, 130, "tbAlbum", new string [] { mf.Album }, 100, 130 );
            AddLabelAndTextBox( canvas, "lblGenre", "Genre", 10, 160, "tbGenre", new string [] { mf.Genre }, 100, 160 );
            AddLabelAndTextBox( canvas, "lblProducers", "Producers", 10, 190, "tbProducers", mf.Producers.ToArray<string>( ), 100, 190 );
            AddLabelAndTextBox( canvas, "lblTrackNumber", "Track Number", 10, 220, "tbTrackNumber", new string [] { mf.Track.ToString( ) }, 100, 220 );
            AddLabelAndTextBox( canvas, "lblTrackCount", "Track Count", 200, 220, "tbTrackCount", new string [] { mf.TrackCount.ToString( ) }, 300, 220 );
            AddLabelAndTextBox( canvas, "lblDiscNumber", "Disc Number", 10, 250, "tbDiscNumber", new string [] { mf.Disc.ToString( ) }, 100, 250 );
            AddLabelAndTextBox( canvas, "lblDiscCount", "Disc Count", 200, 250, "tbDiscCount", new string [] { mf.DiscCount.ToString( ) }, 300, 250 );
            AddLabelAndTextBox( canvas, "lblYear", "Year", 10, 280, "tbYear", new string [] { mf.Year.ToString() }, 100, 280 );
            AddLabelAndTextBox( canvas, "lblBitRate", "BitRate", 10, 310, "tbBitRate", new string [] { mf.BitRate.ToString() + " kB" }, 100, 310, false );
            AddLabelAndTextBox( canvas, "lblLength", "Length", 200, 310, "tbLength", new string [] { mf.Duration.ToString()}, 300, 310, false );
            AddLabelAndTextBox( canvas, "lblFileName", "FileName", 10, 340, "tbFileName", new string [] { mf.PartialFileName }, 100, 340 );

            mediaFileCollection.partialFilePathToMediaFile.Add( mf.PartialFileName, mf );

            Image img = SetMediaFileArtToImage( mf );
            Canvas.SetTop( img, 370 );
            Canvas.SetLeft( img, 10 );
            canvas.Children.Add( img );

            Button btn = new Button( );
            btn.Content = "Save";
            btn.Click += SaveMediaFile;
            Canvas.SetTop( btn, 370 );
            Canvas.SetLeft( btn, 300 );
            canvas.Children.Add( btn );


            Grid.SetRow( canvas, 1 );
            grid.Children.Add( canvas );
            return grid;
        }

        void SaveMediaFile( object sender, RoutedEventArgs e )
        {
            var rn = sender as Button;
            var parent = rn.Parent as Canvas;
            byte [] data = new byte [ 1 ];
            List<TextBox> canvasEntries = new List<TextBox>( );

            foreach ( var item in parent.Children )
            {
                if ( item is TextBox )
                    canvasEntries.Add( (TextBox)item );
                if ( item is Image)
                {
                    data = getBytesFromImageControl( (Image)item );
                }
            }

            var canvasData = new Tuple<List<TextBox>, byte []>( canvasEntries, data );
            WriteMFCanvasTextBoxesToMediaFile( canvasData );
        }

        void WriteMFCanvasTextBoxesToMediaFile(Tuple<List<TextBox>, byte[]> mediaFileCanvasData)
        {
            string  FileName = "", Year = "", DiscCount = "", DiscNumber = "", TrackCount = "", TrackNumber = "", Genre = "", 
                    Album = "", Title = "", Artist = "", Producers ="";

            foreach ( TextBox tb in mediaFileCanvasData .Item1)
            {
                if ( tb.Name == "tbFileName" )
                    FileName = tb.Text;
                if ( tb.Name == "tbTitle" )
                    Title = tb.Text;
                if ( tb.Name == "tbArtist" )
                    Artist = tb.Text;
                if ( tb.Name == "tbAlbum" )
                    Album = tb.Text;
                if ( tb.Name == "tbGenre" )
                    Genre = tb.Text;
                if ( tb.Name == "tbProducers" )
                    Producers = tb.Text;
                if ( tb.Name == "tbTrackNumber" )
                    TrackNumber = tb.Text;
                if ( tb.Name == "tbTrackCount" )
                    TrackCount = tb.Text;
                if ( tb.Name == "tbDiscNumber" )
                    DiscNumber = tb.Text;
                if ( tb.Name == "tbDiscCount" )
                    DiscCount = tb.Text;
                if ( tb.Name == "tbYear" )
                    Year = tb.Text;
            }
            if(mediaFileCollection.partialFilePathToMediaFile.ContainsKey(FileName))
            {
                var mf = mediaFileCollection.partialFilePathToMediaFile [ FileName ];
                mf.Album = Album;
                mf.Artist = Artist;
                mf.Genre = Genre;
                mf.Producers = new string [] { Producers }.ToList<string>( );
                
                if( isNumeric(DiscNumber) )
                    mf.Disc = Convert.ToInt16(DiscNumber);
                if ( isNumeric( DiscCount ) )
                    mf.DiscCount = Convert.ToInt16( DiscCount );
                if ( isNumeric( TrackNumber ) )
                    mf.Track = Convert.ToInt16( TrackNumber );
                if ( isNumeric( TrackCount ) )
                    mf.TrackCount = Convert.ToInt16( TrackCount );
                if ( isNumeric( Year ) )
                    mf.Year = Convert.ToInt16( Year );
                
                if(mediaFileCanvasData.Item2.Length != 1 && mediaFileCanvasData.Item2.Length != 887)
                {
                    mf.SetArtwork(mediaFileCanvasData.Item2);
                }

                
                mf.SaveMediaFile( mf );
            }
            else
                MessageBox.Show( "changing the filename ruins this" );
            
        }

        bool isNumeric( string input )
        {
            int toNumber;
            return int.TryParse( input, out toNumber );
        }

        // Events:
        //          * DragDrop Event for changing the image source
        //          * ContextMenu with 3 items
        //              - Event to change the image source
        //              - Event to view the full size image source
        //              - Event to save the image source as a file
        Image SetMediaFileArtToImage( MediaFile mf)
        {
            Image img = new Image( );

            if ( mf.HasArtwork )
            {
                BitmapImage bmpImage = new BitmapImage( );
                bmpImage.BeginInit( );
                bmpImage.StreamSource = new MemoryStream( mf.GetArtwork( ) [ 0 ] );
                bmpImage.EndInit( );
                img.Source = bmpImage;
            }
            else
            {
                img.Source = CreateImageSource( );
            }
         
            // sizes image -- can change to suit needs
            img.Height = 200;
            img.Width = 200;
            
            // add events to the image control
            img.AllowDrop = true;
            img.DragEnter += DropArtworkOnImage;
            img.ContextMenu = CreateImageContextMenu( );
            img.MouseDown += img_MouseDown;
            return img;
        }

        void img_MouseDown( object sender, MouseButtonEventArgs e )
        {
            if(e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                ViewFullSizeImageInNewWindow( sender, null );
            }
        }

        void img_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            MessageBox.Show("void img_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )", "DAMN BRO");
        }

        // TODO: COMPLETE THE EVENTS
        // Creates a Context Menu with Image specific events
        // Events:
        //          * DragDrop Event for changing the image source
        //          * ContextMenu with 3 items
        //              - Event to change the image source
        //              - Event to view the full size image source
        //              - Event to save the image source as a file
        ContextMenu CreateImageContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu( );
            MenuItem menuItem = new MenuItem( );
            
            menuItem.Header = "Actual Size";
            menuItem.Click += ViewFullSizeImageInNewWindow;
            contextMenu.Items.Add( menuItem );

            menuItem = new MenuItem( );
            menuItem.Header = "Change Image";
            menuItem.Click += ChangeImageSourceWithAnImageFile;
            contextMenu.Items.Add( menuItem );

            menuItem = new MenuItem( );
            menuItem.Header = "Save Image as File";
            menuItem.Click += SaveImageAsFile;
            contextMenu.Items.Add( menuItem );

            return contextMenu;
        }

        // Used with an Image control's context menu
        void SaveImageAsFile( object sender, RoutedEventArgs e )
        {
            MessageBox.Show( "void SaveImageAsFile( object sender, RoutedEventArgs e )", "This feature hasn't been implemented yet" );
        }

        // Used with an Image control's context menu
        void ChangeImageSourceWithAnImageFile( object sender, RoutedEventArgs e )
        {
            MessageBox.Show( "void ChangeImageSourceWithAnImageFile( object sender, RoutedEventArgs e )", "This feature hasn't been implemented yet" );
        }

        // Used with an Image control's context menu
        void ViewFullSizeImageInNewWindow( object sender, RoutedEventArgs e )
        {
            Image img;


            if( sender is MenuItem)
            {
                img = ( ( ( sender as MenuItem ).Parent as ContextMenu ).PlacementTarget ) as Image;
            }
            else if ( sender is Image)
            {
                img = sender as Image;
            }
            else
            {
                return;
            }
                
            if ( img == null ) return;

            var data = getBytesFromImageControl( img );
            
            Image newImg = new Image();
            newImg.Source = ByteToImage(data);
            System.Windows.Window wnd = new Window( );
            wnd.Content = newImg;
            wnd.Height = newImg.Height;
            wnd.Width = newImg.Width;
            wnd.Show( );
        }

        
        void DropArtworkOnImage( object sender, DragEventArgs e )
        {
            Image img = sender as Image;
            if ( img == null ) return;

            bool endEarly = true;

            bool isFile = e.Data.GetDataPresent(DataFormats.FileDrop);

            if ( isFile && img != null )
            {
                string[] file = (string[])e.Data.GetData( DataFormats.FileDrop );
                
                
                if ( ImageExtensions.Contains( System.IO.Path.GetExtension( file[0] ).ToUpperInvariant( ) ) )
                {
                    img.Source = CreateImageSource( file [ 0 ] );
                    if ( endEarly == true ) endEarly = false;
                }
            }
            
            if ( endEarly == true ) return;
            
            var imgParent = img.Parent as Canvas;
            if(imgParent.Name != "albumCanvas")
            {
                foreach ( var item in imgParent.Children )
                {
                    if ( item is TextBox )
                    {
                        TextBox tb = item as TextBox;

                        if ( tb.Name == "tbFileName" )
                        {
                            mediaFileCollection.partialFilePathToMediaFile [ tb.Text ].SaveCanvasImageSource = true;
                            mediaFileCollection.partialFilePathToMediaFile [ tb.Text ].SetArtwork( getBytesFromImageControl( img ) );
                        }
                    }
                }
            }

        }
        
        public byte[] getBytesFromImageControl(Image img)
        {
            BitmapSource bmpSource = (BitmapSource)img.Source;

            JpegBitmapEncoder encoder = new JpegBitmapEncoder( );
            encoder.Frames.Add( BitmapFrame.Create( bmpSource ) );
            encoder.QualityLevel = 100;
            byte [] data;
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Frames.Add( BitmapFrame.Create( bmpSource ) );
                encoder.Save( ms );
                data = ms.ToArray( );
                ms.Close( );
            }
            return data;
        }

        // return value to allow for contextual offsetting for multiple line entries
        int AddLabelAndTextBox( Canvas parentControl ,string labelName, string labelContent, int labelXPos, int labelYPos, string textboxName, string[] textboxContent, int textboxXpos, int textboxYPos, bool tbWritable = true)
        {
            Label lbl = new Label( );
            TextBox tb = new TextBox( );

            lbl.Name = labelName;
            lbl.Content = labelContent;
            Canvas.SetLeft( lbl, labelXPos );
            Canvas.SetTop( lbl, labelYPos );
            parentControl.Children.Add( lbl );

            tb.Name = textboxName;
            foreach(string content in textboxContent)
            {
                tb.Text += content;
                if ( content != textboxContent [ textboxContent.Length - 1 ] )
                    tb.Text += ", ";
            }
            Canvas.SetLeft( tb, textboxXpos );
            Canvas.SetTop( tb, textboxYPos );
            if(tbWritable == false)
            {
                tb.Background = System.Windows.Media.Brushes.DarkGray;
                tb.IsReadOnly = true;
            }
            parentControl.Children.Add( tb );

            return textboxContent.Length;
        }

        // Creates a TabControl for all Writeable Results from the scanned item
        // If downloadArtwork is true, then this function will query http://coverartarchive.org for the
        // file's artwork based on it's MusicBrainz ReleaseID
        TabControl CreateWriteableResultTabControl( WriteableResult scannedFile, MediaFile mf, bool downloadArtwork )
        {
            TabControl tc = new TabControl( );

            try
            {
                tc.Name = "tcWriteableResultOptions";
                // for format testing
                tc.Background = System.Windows.Media.Brushes.CadetBlue;

                // AcoustID scanner logic, hardcoded constructor arguments
                var tabItems = CreateResultOptionTabItems( scannedFile.WriteableResultOptions, mf, downloadArtwork );

                // If there are writeable results, then each result tabitem will be added to the TabControl
                if ( tabItems.Count > 0 )
                {
                    foreach ( TabItem tabItem in tabItems )
                    {
                        tc.Items.Add( tabItem );
                    }
                }
                else
                {
                    try
                    {
                        var zeroResultTabItem = CreateZeroResultOptionTabItem( mf );
                        tc.Items.Add( zeroResultTabItem );
                        SetTabControlStyleToHideTabItemHeaders( tc );
                        tc.SelectedItem = tc.Items [ 0 ];
                    }
                    catch
                    {
                        MessageBox.Show("hhhh");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show( ex.Message + System.Environment.NewLine + ex.StackTrace );
            }
      
            return tc;
        }

        // Creates a list of result option tab items for each WriteableResultOption in the list parameter
        // The counter is to distinguish tabitem headers, starting from "Result 1" up to "Result N", where N is the final loop iteration
        // TODO: List<WriteableResultOption> sorting to allow for prioritization of relevant result ordeirng (e.g., earlier releases dates first )
        List<TabItem> CreateResultOptionTabItems( List<WriteableResultOption> wros, MediaFile mf, bool downloadArtwork )
        {
            var tabItems = new List<TabItem>();
            int counter = 0;
            foreach(WriteableResultOption wro in wros)
            {
                tabItems.Add( CreateResultOptionTabItem( wro, mf, ++counter, downloadArtwork) );
            }
            return tabItems;
        }

        // Creates a ResultOption TabItem for a given WriteableResultOption
        TabItem CreateResultOptionTabItem(WriteableResultOption wro, MediaFile mf, int tabItemNumber, bool downloadArtwork)
        {
            // Creates 
            TabItem tabItem = new TabItem( );
            tabItem.Header = "Result " + tabItemNumber.ToString();
           
            Grid mainResultOptionGrid = new Grid( );
            mainResultOptionGrid.RowDefinitions.Add( new RowDefinition( ) );
            mainResultOptionGrid.RowDefinitions.Add( new RowDefinition( ) );
            
            // coloration tenatively in place for control separation for debugging purposes
            mainResultOptionGrid.Background = System.Windows.Media.Brushes.CornflowerBlue;
            Canvas albumCanvas = CreateAlbumSpecificCanvas( wro, mf, System.Windows.Media.Brushes.DarkSalmon, true );
            Canvas trackCanvas = CreateTrackSpecificCanvas( wro, mf, System.Windows.Media.Brushes.DarkSeaGreen, true );
            mainResultOptionGrid.Children.Add( albumCanvas );
            mainResultOptionGrid.Children.Add( trackCanvas );
           
            tabItem.Content = mainResultOptionGrid;
            return tabItem;
        }

        // Creates a ResultOption TabItem for a file that returns zeor WriteableResultOptions
        TabItem CreateZeroResultOptionTabItem( MediaFile mf )
        {
            TabItem tabItem = new TabItem( );
            tabItem.Header = "No Results";
            Grid mainResultOptionGrid = new Grid( );
            mainResultOptionGrid.RowDefinitions.Add( new RowDefinition( ) );
            mainResultOptionGrid.RowDefinitions.Add( new RowDefinition( ) );

            WriteableResultOption blankWriteableResultOption = new WriteableResultOption(mf.FileName);
                        
            // coloration tentatively in place for control separation for under construction design/debugging purposes
            mainResultOptionGrid.Background = System.Windows.Media.Brushes.CornflowerBlue;
        
            try
            {
                Canvas albumCanvas = CreateAlbumSpecificCanvas( blankWriteableResultOption, mf, System.Windows.Media.Brushes.DarkSalmon, false );
                Canvas trackCanvas = CreateTrackSpecificCanvas( blankWriteableResultOption, mf, System.Windows.Media.Brushes.DarkSeaGreen, false );
                mainResultOptionGrid.Children.Add( albumCanvas );
                mainResultOptionGrid.Children.Add( trackCanvas );
            }
            catch(Exception ex)
            {
                ExceptionList.Add( new Tuple<string, string>( ex.Message , ex.StackTrace) );
            }
            
            tabItem.Content = mainResultOptionGrid;
            return tabItem;
        }

        // Creates and returns a Canvas containing the WriteableResultOption fields pertintent to album entries
        // Hardcoded to be assigned to the first row of the mainResultOptionGrid in TabItem CreateResultOptionTabItem()
        Canvas CreateAlbumSpecificCanvas(WriteableResultOption wro, MediaFile mf , System.Windows.Media.Brush background, bool downloadArtwork)
        {
            Canvas albumCanvas = new Canvas( );
            albumCanvas.Name = "albumCanvas";
            Label lbl = new Label( );
            TextBox tb = new TextBox( );

            lbl.Name = "lblFileName";
            lbl.Content = mf.PartialFileName;
            lbl.Visibility = Visibility.Hidden;
            albumCanvas.Children.Add( lbl );
            
            albumCanvas.Background = background;

            // Album Items
            lbl = new Label( );
            lbl.Content = "Album";
            Canvas.SetTop( lbl, 10 );
            Canvas.SetLeft( lbl, 10 );
            albumCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbAlbum";
            tb.Text = wro.Album;
            Canvas.SetTop( tb, 10 );
            Canvas.SetLeft( tb, 100 );
            albumCanvas.Children.Add( tb );

            lbl = new Label( );
            lbl.Content = "Track Count";
            Canvas.SetTop( lbl, 40 );
            Canvas.SetLeft( lbl, 10 );
            albumCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbTrackCount";
            tb.Text = wro.TrackCount.ToString( );
            Canvas.SetTop( tb, 40 );
            Canvas.SetLeft( tb, 100 );
            albumCanvas.Children.Add( tb );

            lbl = new Label( );
            lbl.Content = "Disc Count";
            Canvas.SetTop( lbl, 40 );
            Canvas.SetLeft( lbl, 200 );
            albumCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbDiscCount";
            tb.Text = wro.DiscCount.ToString( );
            Canvas.SetTop( tb, 40 );
            Canvas.SetLeft( tb, 300 );
            albumCanvas.Children.Add( tb );

            lbl = new Label( );
            lbl.Content = "Year";
            Canvas.SetTop( lbl, 70 );
            Canvas.SetLeft( lbl, 10 );
            albumCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbYear";
            tb.Text = wro.Year.ToString( );
            Canvas.SetTop( tb, 70 );
            Canvas.SetLeft( tb, 100 );
            albumCanvas.Children.Add( tb );

            lbl = new Label( );
            lbl.Content = "Genre";
            Canvas.SetTop( lbl, 70 );
            Canvas.SetLeft( lbl, 200 );
            albumCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbGenre";
            tb.Text = "";
            Canvas.SetTop( tb, 70 );
            Canvas.SetLeft( tb, 300 ); ;
            albumCanvas.Children.Add( tb );


            if ( downloadArtwork == true )
            {
                ScannedFiles.SetArtwork( mf );
            }

            if ( wro.ReleaseID != null && ScannedFiles.ReleaseIDToImageData.ContainsKey( wro.ReleaseID )  )
            {
                try
                {
                    Image img = new Image( );
                    BitmapImage bmpImage = new BitmapImage( );
                    bmpImage.BeginInit( );
                    bmpImage.StreamSource = new MemoryStream( ScannedFiles.ReleaseIDToImageData [ wro.ReleaseID ] );
                    bmpImage.DecodePixelHeight = 170;
                    bmpImage.DecodePixelWidth = 170;
                    bmpImage.EndInit( );
                    img.Source = bmpImage;
                    Canvas.SetTop( img, 100 );
                    Canvas.SetLeft( img, 10 );
                    img.Height = 160;
                    img.Width = 160;
                    img.AllowDrop = true;
                    img.Drop += DropArtworkOnImage;
                    albumCanvas.Children.Add( img );
                }
                catch ( Exception ex )
                {
                    ExceptionList.Add( new Tuple<string, string>( wro.ReleaseID + " - > to image data -> to Image control", ex.Message + System.Environment.NewLine + ex.StackTrace ) );
                }
            }

            // Adds controls for files without any writeable result options
            if(wro.IsANullResult == true )
            {
          
                lbl = new Label( );
                lbl.Content = "Nothing found for " + mf.FileName;
                lbl.FontSize = 9;
                Canvas.SetTop( lbl, 100 );
                Canvas.SetLeft( lbl, 10 );
                albumCanvas.Children.Add( lbl );
            }

            //removed wro.IsANullResult == true || 
            if(wro.ImageSourceURL == null)
            {
                Image img = new Image( );
                img.Source = CreateImageSource( );
                img.Height = 140;
                img.Width = 140;
                // TODO: ADD EVENTS, BRO
                img.DragOver += DropArtworkOnImage;
                img.AllowDrop = true;    
                // TODO: COMPLETE

                Canvas.SetTop( img, 140 );
                Canvas.SetLeft( img, 10 );
                albumCanvas.Children.Add( img );
            }

            // Create button to apply album specific data for a given resultoption tab to every file in the ( FOR NOW // TODO ALTER THIS ) containing folder
            Button btnApplyToAllFilesInFolder = new Button( );
            btnApplyToAllFilesInFolder.Content = "Apply to All Files in the Containing Folder";
            btnApplyToAllFilesInFolder.Click += SaveAllFilesInContainingFolder;
            Canvas.SetTop( btnApplyToAllFilesInFolder, 140 );
            Canvas.SetLeft( btnApplyToAllFilesInFolder, 200 );
            albumCanvas.Children.Add( btnApplyToAllFilesInFolder );

            // End of Album Items Canvas
            Grid.SetRow( albumCanvas, 0 );
            return albumCanvas;
        }

        void SaveAllFilesInContainingFolder( object sender, RoutedEventArgs e )
        {
            Button button = sender as Button;
            Canvas albumCanvas = button.Parent as Canvas;
            var canvasData = ExtractAlbumCanvasData( albumCanvas );

            foreach ( var item in albumCanvas.Children )
            {
                Label lbl = item as Label;

                if ( lbl != null )
                {
                    if ( lbl.Name == "lblFileName" )
                    {
                        var fileName = ( item as Label ).Content;
                        var currentMF = mediaFileCollection.partialFilePathToMediaFile [ (string)fileName ];
                        var containingListBoxName = FolderToListBoxNameMapping [ currentMF.ContainingFolder ];
                        ListBox containgFolderLB = FolderListboxMapping [ containingListBoxName ];

                        try
                        {
                            foreach ( var file in containgFolderLB.Items )
                            {
                                var mf = mediaFileCollection.partialFilePathToMediaFile [ (string)file ];
                                if ( mf is MediaFile )
                                {
                                    
                                    if(MediaFileNameToMediaFileGrid.ContainsKey(mf.FileName))
                                    {
                                        UpdateMediaFileCanvasData_Album(MediaFileNameToMediaFileGrid[mf.FileName], canvasData);
                                    }
                                    

                                    Task.Factory.StartNew( () =>
                                        {
                                            WriteAlbumSpecificMediaFileData( (MediaFile)mf, canvasData );
                                        } );
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show( ex.Message + "\r\n" + ex.StackTrace );
                        }
                    }
                }
            }
        }

        void WriteAlbumSpecificMediaFileData(MediaFile mf, Tuple<string, int, int, int, string, byte[]> cd )
        {
            mf.Album = cd.Item1;
            mf.TrackCount = cd.Item2;
            mf.DiscCount = cd.Item3;
            mf.Year = cd.Item4;
            mf.Genre = cd.Item5;
            mf.SetArtwork( cd.Item6 );
            mf.SaveMediaFile( mf );
        }

        void WriteTrackSpecificMediaFileData(MediaFile mf, Tuple<string, int, int, string, List<string>> cd)
        {
            mf.Title = cd.Item1;
            mf.Track = cd.Item2;
            mf.Disc = cd.Item3;
            // TODO: RENAME WITH FILE AS WELL; UPDATE MAPPINGS -- THIS IS JUST A DUMMY PLACEHOLDER AS IS 03/02/2014
            mf.FileName = cd.Item4;
            // TODO: IF CHANGE HOW ARTISTS WORK, CHANGE THIS
            mf.Artist = cd.Item5 [ 0 ];
            mf.SaveMediaFile( mf );
        }


        BitmapImage ByteToImage(byte[] data)
        {
            BitmapImage bmpImg = new BitmapImage( );
            MemoryStream ms = new MemoryStream( data );
            bmpImg.BeginInit( );
            bmpImg.StreamSource = ms;
            bmpImg.EndInit( );

            return bmpImg;
        }


        void UpdateMediaFileCanvasData_Track( Grid mfGrid, Tuple<string, int, int, string, List<string>> trackData )
        {
            foreach ( var item in mfGrid.Children )
            {
                if (item is Canvas)
                {
                    Canvas canvas = (Canvas)item;

                    foreach(var canvasItem in canvas.Children)
                    {
                        if ( canvasItem is TextBox )
                        {
                            TextBox tb = (TextBox)canvasItem;
                            if ( tb.Name == "tbTitle" )
                                tb.Text = trackData.Item1;
                            if ( tb.Name == "tbTrackNumber" )
                                tb.Text = trackData.Item2.ToString( );
                            if ( tb.Name == "tbDiscNumber" )
                                tb.Text = trackData.Item3.ToString( );
                            if ( tb.Name == "tbFileName" )
                                tb.Text = trackData.Item4.ToString( );
                            // TODO: fix this once the artists inconsistency is fixed
                            if ( tb.Name == "tbArtist" )
                                tb.Text = trackData.Item5 [ 0 ];
                        }
                    }
                }

            }
        }

        // Item 1: Title, Item2: TrackNumber, Item3: DiscNumber
        // Item 4: FileName, Item 5: Artists
        Tuple<string, int, int, string, List<string>> ExtractTrackCanvasData(Canvas canvas)
        {
            string title = "";
            int trackNumber = 0;
            int discNumber = 0;
            string fileName = "";
            List<string> artists = new List<string>( );


            foreach(var item in canvas.Children)
            {
                if(item is TextBox)
                {
                    TextBox tb = item as TextBox;

                    if ( tb.Name == "tbTitle" )
                        title = tb.Text;
                    if ( tb.Name == "tbTrackNumber" )
                        trackNumber = Convert.ToInt16( tb.Text );
                    if ( tb.Name == "tbDiscNumber" )
                        discNumber = Convert.ToInt16(tb.Text);
                    if ( tb.Name == "tbFileName" )
                        fileName = tb.Text;
                    if ( tb.Name == "tbArtist" )
                    {
                        artists.Add( tb.Text );
                    }
                        
                }
            }

            var canvasData = new Tuple<string, int, int, string, List<string>>( title, trackNumber, discNumber, fileName, artists );
            return canvasData;
        }

        // Item 1: Album, Item2: TrackCount, Item3: DiscCount
        // Item 4: Year, Item5: Genre, Item6: Picture
        Tuple<string, int, int, int, string, byte[]> ExtractAlbumCanvasData(Canvas canvas)
        {
            string album = "";
            string genre = "";
            int trackCount = 0;
            int discCount = 0;
            int year = 0;
            byte[] data = new byte[1];

            foreach(var item in canvas.Children)
            {
                if( item is TextBox)
                {
                    TextBox tb = item as TextBox;

                    if ( tb.Name == "tbAlbum" )
                        album = tb.Text;
                    if ( tb.Name == "tbGenre" )
                        genre = tb.Text;
                    
                    if ( tb.Name == "tbTrackCount")
                    {
                        if(isNumeric(tb.Text))
                        {
                            trackCount = Convert.ToInt16( tb.Text );
                        }
                    }
                    
                    if ( tb.Name == "tbDiscCount" )
                    {
                        if ( isNumeric( tb.Text ) )
                        {
                            discCount = Convert.ToInt16( tb.Text );
                        }
                    }
                    
                    if ( tb.Name == "tbYear" )
                    {
                        if ( isNumeric( tb.Text ) )
                        {
                            year = Convert.ToInt16( tb.Text );
                        }
                    }
                }
                if ( item is Image)
                {
                    Image img = item as Image;
                    data = getBytesFromImageControl( img );
                }
            }
 
            var canvasData = new Tuple<string, int, int, int, string, byte []>( album, trackCount, discCount, year, genre, data );
            return canvasData;
        }

        // Creates and returns a Canvas containing the WriteableResultOption fields pertintent to specific track entries
        // Hardcoded to be assigned to the second row of the mainResultOptionGrid in TabItem CreateResultOptionTabItem()
        Canvas CreateTrackSpecificCanvas(WriteableResultOption wro, MediaFile mf , System.Windows.Media.Brush background, bool downloadArtwork)
        {
            Canvas trackCanvas = new Canvas( );
            trackCanvas.Name = "trackCanvas";
            Label lbl = new Label( );
            TextBox tb = new TextBox( );

            trackCanvas.Background = background;
            // Track Items

            lbl = new Label( );
            lbl.Content = "Title";
            Canvas.SetTop( lbl, 10 );
            Canvas.SetLeft( lbl, 10 );
            trackCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbTitle";
            tb.Text = wro.Title;
            Canvas.SetTop( tb, 10 );
            Canvas.SetLeft( tb, 100 );
            trackCanvas.Children.Add( tb );

            lbl = new Label( );
            lbl.Content = "Track Number";
            Canvas.SetTop( lbl, 40 );
            Canvas.SetLeft( lbl, 10 );
            trackCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbTrackNumber";
            tb.Text = wro.TrackNumber.ToString( );
            Canvas.SetTop( tb, 40 );
            Canvas.SetLeft( tb, 100 );
            trackCanvas.Children.Add( tb );
            
            lbl = new Label( );
            lbl.Content = "Disc Number";
            Canvas.SetTop( lbl, 40 );
            Canvas.SetLeft( lbl, 200 );
            trackCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbDiscNumber";
            tb.Text = wro.DiscNumber.ToString( );
            Canvas.SetTop( tb, 40 );
            Canvas.SetLeft( tb, 300 );
            trackCanvas.Children.Add( tb );

            lbl = new Label( );
            lbl.Content = "FileName";
            Canvas.SetTop( lbl, 70 );
            Canvas.SetLeft( lbl, 10 );
            trackCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbFileName";
            tb.Text = GenericFileName( wro.Title, wro.Artists, wro.TrackNumber );
            Canvas.SetTop( tb, 70 );
            Canvas.SetLeft( tb, 100 );
            trackCanvas.Children.Add( tb );


            lbl = new Label( );
            lbl.Content = "Artists";
            Canvas.SetTop( lbl, 100 );
            Canvas.SetLeft( lbl, 10 );
            trackCanvas.Children.Add( lbl );
            tb = new TextBox( );
            tb.Name = "tbArtist";

            foreach ( string artist in wro.Artists )
            {
                tb.Text += artist + System.Environment.NewLine;
            }

            Canvas.SetTop( tb, 100 );
            Canvas.SetLeft( tb, 100 );
            trackCanvas.Children.Add( tb );


            // TODO: completely implement this button
            // For now it's hidden as it's not correctly functional
            Button btn = new Button( );
            btn.Content = "Save Track";
            btn.Click += SaveTrackData;
            btn.Visibility = Visibility.Hidden;
            Canvas.SetTop( btn, 200 );
            Canvas.SetLeft( btn, 200 );
            trackCanvas.Children.Add( btn );


            Grid.SetRow( trackCanvas, 1 );

            return trackCanvas;
        }

        MediaFile GetMediaFileFromSelectedItem(string lbFoldersSelectedItem)
        {
            string listBoxName = FolderToListBoxNameMapping [ lbFoldersSelectedItem ];
            ListBox lb = FolderListboxMapping [ listBoxName ];
            if ( lb.SelectedItem == null )
            {
                if ( lb.Items.Count == 0 )
                    return null;
                
                lb.SelectedItem = lb.Items [ 0 ];
            }
                
            string partialFileName = (string)lb.SelectedItem;
            MediaFile mf = mediaFileCollection.partialFilePathToMediaFile [ partialFileName ];
            return mf;
        }

        void SaveTrackData( object sender, RoutedEventArgs e )
        {
            Button btn = sender as Button;
            Canvas trackCanvas = btn.Parent as Canvas;
            Grid parentGrid = trackCanvas.Parent as Grid;


            MediaFile mf = GetMediaFileFromSelectedItem( (string)lbFolders.SelectedItem );
            // cannot save data with a null Media File
            if ( mf == null ) return;

            //lbFolders.SelectedItem
            
            var fileFolderName = FolderToListBoxNameMapping[(string)lbFolders.SelectedItem];

            var relevantLB = FolderListboxMapping [ (string)fileFolderName ];
            var fileName = relevantLB.SelectedItem;

            foreach(var item in parentGrid.Children)
            {
                if( item is Canvas)
                {
                    Canvas tmpCanvas = (Canvas)item;
                    if( tmpCanvas.Name == "albumCanvas")
                    {
                        var canvasData = ExtractAlbumCanvasData( tmpCanvas );
                        if ( !isBlankCanvasAlbumData( canvasData ) )
                        {
                            UpdateMediaFileCanvasData_Album( MediaFileNameToMediaFileGrid [ mf.FileName ], canvasData );
                            WriteAlbumSpecificMediaFileData( mf, canvasData );
                        }
                            
                    }
                    if ( tmpCanvas.Name == "trackCanvas")
                    {
                        var canvasData = ExtractTrackCanvasData( tmpCanvas );
                        if ( !isBlankCanvasTrackData( canvasData ) )
                        {
                            UpdateMediaFileCanvasData_Track( MediaFileNameToMediaFileGrid [ mf.FileName ], canvasData );
                            WriteTrackSpecificMediaFileData( mf, canvasData );
                        }

                        
                    }
                }
            }
        }



        void UpdateMediaFileCanvasData_Album( Grid mfGrid, Tuple<string, int, int, int, string, byte []> albumData )
        {
            foreach ( var item in mfGrid.Children )
            {
                if ( item is Canvas )
                {
                    Canvas canvas = (Canvas)item;

                    foreach ( var canvasItem in canvas.Children )
                    {

                        if ( canvasItem is TextBox )
                        {
                            TextBox tb = (TextBox)canvasItem;

                            if ( tb.Name == "tbAlbum" )
                                tb.Text = albumData.Item1;
                            if ( tb.Name == "tbTrackCount" )
                                tb.Text = albumData.Item2.ToString( );
                            if ( tb.Name == "tbDiscCount" )
                                tb.Text = albumData.Item3.ToString( );
                            if ( tb.Name == "tbYear" )
                                tb.Text = albumData.Item4.ToString( );
                            if ( tb.Name == "tbGenre" )
                                tb.Text = albumData.Item5.ToString( );
                        }
                        if ( canvasItem is Image )
                        {
                            Image img = (Image)canvasItem;
                            img.Source  =  ByteToImage( albumData.Item6 );
                        }
                    }

                }

            }
        }

        // wrapper function for this boolean condition logic
        bool isBlankCanvasAlbumData(Tuple<string, int, int, int, string, byte[]> albumCanvasData)
        {
            // the default image byte is expected to have a length of 887
            if ( albumCanvasData.Item1 == "" && albumCanvasData.Item2 == 0 && albumCanvasData.Item3 == 0 && albumCanvasData.Item4 == 0 && albumCanvasData.Item5 == "" && albumCanvasData.Item6.Length == 887 )
                return true;
            else
                return false;
        }

        // wrapper function for this boolean condition logic
        bool isBlankCanvasTrackData(Tuple<string, int, int, string, List<string>> trackCanvasData)
        {
            // ignores item4 which is a function of the other items in this tuple
            if ( trackCanvasData.Item1 == "" && trackCanvasData.Item2 == 0 && trackCanvasData.Item3 == 0 && trackCanvasData.Item5.Count == 0 )
                return true;
            else
                return false;
        }

        // Creates and returns a file name for each scanned result options based on the parameters
        string GenericFileName(string title, List<string> artists, int? trackNumber)
        {
            if(trackNumber == null)
            {
                trackNumber = 0;
            }

            string FileName;

            if(trackNumber < 10)
            {
                FileName = "0" + trackNumber.ToString( ) + " - ";
            }
            else
            {
                FileName = trackNumber.ToString( ) + " - ";
            }

            FileName += title;

            // input for Artists: artist a Title: Song 1, TrackNumber: 1
            // output: 01 - Song 1 
            // input for Artists: artist a, artist b, artist c, Title: Song 1, TrackNumber: 1
            // output: 01 - Song 1 ( feat. artist a, artist b )
            if ( artists.Count < 2 )
            {
                return FileName;
            }
            else
            {
                FileName += " (feat. ";

                for ( int i = 1; i < artists.Count; i++ )
                {
                    if ( i != 1 )
                        FileName += ", ";

                    FileName += artists [ i ];
                }
                
                FileName += ")";
                return FileName;
            }
        }

        // Adds an associated artwork for each folder directory, if one can be found
        //  searches in the directory containing the files and the parent directory
        // TODO: toggle options
        BitmapSource CreateImageSource(string imagePath = "")
        {
            if ( File.Exists( imagePath ) )
            {
                return new BitmapImage( new Uri( imagePath ) );
            }
            else if ( imagePath == "" )
            {
                int width = 128;
                int height = width;
                int stride = width / 8;
                byte [] pixels = new byte [ height * stride ];
                // Try creating a new image with a custom palette.
                List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>( );
                colors.Add( System.Windows.Media.Colors.Red );
                colors.Add( System.Windows.Media.Colors.Blue );
                colors.Add( System.Windows.Media.Colors.Green );
                BitmapPalette myPalette = new BitmapPalette( colors );
                // Creates a new empty image with the pre-defined palette
                BitmapSource image = BitmapSource.Create( width, height, 96, 96, PixelFormats.Indexed1, 
                                                          myPalette, pixels, stride );
                return image;
            }
            else
                return null;
        }

        // primary
        // must be set after SetGridName (dependency)
        string SetGridName(string folderName, Grid grid)
        {
            // same name used for associated listbox
            // same intended mapping
            // the - 1 is to offset the mapping count from setting the listbox name
            string gridName = "Item" + ( FolderToListBoxNameMapping.Count - 1 ).ToString( );
            
            FolderGridMapping.Add( gridName, grid );
            return gridName;
        }
     
        // sub
        // must be set before SetGridName (dependency)
        string SetListBoxName( string folderPath, ListBox listBox )
        {
            // same name used for associated grid
            string lbName = "Item" + ( FolderToListBoxNameMapping.Count ).ToString( );
            FolderToListBoxNameMapping.Add( folderPath, lbName );
            FolderListboxMapping.Add( lbName, listBox );
            return lbName;
        }

        // primary
        string SetSubGridName( string fileName, Grid grid)
        {
            string sgName = "Item" + ( FileToGridNameMapping.Count - 1 ).ToString( );

            FileGridMapping.Add( sgName, grid );
            return null;
        }

        // sub
        string SetTabItemName( string fileName, TabItem tabItem )
        {
            string tabItemName= "Item" + ( FileToGridNameMapping.Count ).ToString( );
            FileToGridNameMapping.Add( fileName, tabItemName );
            FileTabItemMapping.Add( tabItemName, tabItem );
            return tabItemName;
        }

        // Creates a listbox with the name: "GridItem" + [ NumberOfExistingListBoxes + 1]
        // and assigns the lbFiles_SelectionChanged SelectionedChanged event 
        // internally, it adds the following mapping:
        //
        //  types:  string     -> string      -> ListBox
        //  values: folderName -> listBoxName -> listBox
        //  depends on the following form class variables:
        //  * FolderToListBoxNameMapping
        //  * FolderListboxMapping
        //  depends on the following form class functions:
        // 
        ListBox CreateFolderListBox(string folderName)
        {
            ListBox lb = new ListBox( );
            lb.Name = SetListBoxName( folderName, lb );
            lb.SelectionChanged += lbFiles_SelectionChanged;
            return lb;
        }

        // Wrapper to sanitize the creation of MediaFiles
        MediaFile CreateMediaFile(string fileName)
        {
            return new MediaFile( TagLib.File.Create( fileName ) ); ;
        }

        // Invokes ProcessFile() on multiple files/folders
        void ProcessFiles(string[] files, bool downloadArtwork = true)
        {
            foreach ( string file in files )
            {
                if ( MediaFileExtensions.Contains( System.IO.Path.GetExtension( file ).ToUpperInvariant( ) ) )
                {
                    ProcessFile( file, downloadArtwork );
                }
            }
                
        }

        void ProcessFile(string file, bool downloadArtwork = true)
        {
            if(File.Exists(file))
            {
                try
                {
                    MediaFile mf = CreateMediaFile( file );
                    mediaFileCollection.ocMediaFiles.Add( mf ); 
                    bool containsListBox = lbFolders.Items.Contains(mf.ContainingFolder);
                    bool doesNotContainListBox = !containsListBox;
                    
                    if( !ScannedFiles.MediaFileWritableResults.ContainsKey(mf.FileName))
                    {
                        ScannedFiles.Add( mf );
                    }
                        
                    Dispatcher.Invoke( () =>
                        {
                            if(doesNotContainListBox)
                            {
                                try
                                {
                                    tcFiles.Items.Add( CreateFolderGridTabItem( mf ) );
                                    lbFolders.Items.Add( mf.ContainingFolder );

                                    // the save track function depends on lbFolders.Selected
                                    // this is to guard against conditions where the user tries to save the default displayed tabcontrol
                                    // which is only expected to result for the default case
                                    if ( lbFolders.SelectedItem == null )
                                        lbFolders.SelectedItem = lbFolders.Items [ 0 ];

                                    var filesListBox = FolderListboxMapping [ GetListBoxName( mf.ContainingFolder ) ];
                                    if ( !filesListBox.Items.Contains( mf.PartialFileName )  )
                                    {
                                        filesListBox.Items.Add( mf.PartialFileName );

                                        if ( ScannedFiles.MediaFileWritableResults.ContainsKey( mf.FileName ) )
                                        {
                                            tcFileInformation.Items.Add( CreateFileTabItem( ScannedFiles.MediaFileWritableResults [ mf.FileName ], mf, downloadArtwork ) );
                                            
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ExceptionList.Add( new Tuple<string, string>(mf.PartialFileName + " , " + mf.ContainingFolder, ex.Message + System.Environment.NewLine + ex.StackTrace ));
                                }
                            }
                            else if (containsListBox)
                            {
                                try
                                {
                                    var filesListBox = FolderListboxMapping [ GetListBoxName( mf.ContainingFolder ) ];
                                    if ( !filesListBox.Items.Contains( mf.PartialFileName ) )
                                    {
                                        filesListBox.Items.Add( mf.PartialFileName );

                                        if ( ScannedFiles.MediaFileWritableResults.ContainsKey( mf.FileName ) )
                                        {
                                            tcFileInformation.Items.Add( CreateFileTabItem( ScannedFiles.MediaFileWritableResults [ mf.FileName ], mf, downloadArtwork ) );
                                        }
                                    }
                                }
                                catch ( Exception ex )
                                {
                                    ExceptionList.Add( new Tuple<string, string>( mf.PartialFileName + " , " + mf.ContainingFolder, ex.Message + System.Environment.NewLine + ex.StackTrace ) );
                                }
                            }
                        } );
                }
                catch(Exception ex)
                {
                    Dispatcher.Invoke( () =>
                        {
                            // TODO: ADD REALTIME EXCEPTION NOTIFICATIONS


                        } );
                }
            }
        }

        string GetListBoxName(string folderPath)
        {
            return FolderToListBoxNameMapping [ folderPath ];
        }

        string GetTabItemName(string fileName)
        {
            try
            {
                return FileToGridNameMapping [ fileName ];
            }
            catch(Exception ex)
            {
                ExceptionList.Add( new Tuple<string, string>( "FileToGridNameMapping[] -> " + fileName, ex.Message + System.Environment.NewLine + ex.StackTrace ) );
                return null;
            }
            
        }

        private void lbFolders_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            ListBox sendingLB = (ListBox)sender;
            var selectedItem = (string)sendingLB.SelectedItem;
            var wrItems = tcFiles.Items;
            var mfItems = tcFileInformation.Items;

            foreach( var item in tcFiles.Items)
            {
                if(item is Grid)
                {
                    Grid lb = (Grid)item;
                    if ( lb.Name == GetListBoxName(selectedItem) )
                    {
                        tcFiles.SelectedItem = lb;
                        tcFileInformation.SelectedItem = lb;
                        var tcFilesListBox = FolderListboxMapping [ lb.Name ];
                        
                        if( tcFilesListBox.Items.Count != 0)
                            tcFilesListBox.SelectedItem = tcFilesListBox.Items [ 0 ];
                        lbFiles_SelectionChanged( tcFilesListBox, null );
                        break;
                    }
                }
            }
        }

        private void lbFiles_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            ListBox sendingLB = (ListBox)sender;
            var selectedItem = (string)sendingLB.SelectedItem;
            var tcItems = tcFileInformation.Items;
            var tcItemsNo = tcItems.Count;
            var query = GetTabItemName( selectedItem );
            try
            {
                foreach(var item in tcFileInformation.Items)
                {
                    if(item is TabItem)
                    {
                        TabItem tb = (TabItem)item;
                        var tbName = tb.Name;
                        if(tbName == GetTabItemName(selectedItem))
                        {
                            tcFileInformation.SelectedItem = tb;
                            break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ExceptionList.Add( new Tuple<string, string>( "lbFiles_SelectionChanged: " +  selectedItem, ex.Message + System.Environment.NewLine + ex.StackTrace ) );
            }
        }

        private void lbFolders_DragEnter( object sender, DragEventArgs e )
        {
            var material = sender;
            var theRecipe = e;
        }
    }
}
