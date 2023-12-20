using ComPDFKit.PDFDocument;
using Compdfkit_Tools.Data;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Compdfkit_Tools.PDFView;

namespace AnnotationViewControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window, INotifyPropertyChanged
    {
        #region Properties
        
        private string currentMode = "Annotation";
        private CPDFDisplaySettingsControl displaySettingsControl = new CPDFDisplaySettingsControl();
        private RegularViewerControl regularViewerControl = new RegularViewerControl();
        private PDFViewControl pdfViewer;
        private PDFViewControl passwordViewer;
        private AnnotationControl annotationControl = new AnnotationControl();
        private CPDFBOTABarControl botaBarControl = new CPDFBOTABarControl();
        private PanelState panelState = PanelState.GetInstance();
        
        private bool _canSave = false;
        public bool CanSave
        {

            get => _canSave;
            set
            {
                _canSave = value;
                OnPropertyChanged();
            }
        }
        
        public bool LeftToolPanelButtonIsChecked
        {
            get => panelState.IsLeftPanelExpand;
            set
            {
                panelState.IsLeftPanelExpand = value;
                OnPropertyChanged();
            }
        }

        public bool RightToolPanelButtonIsChecked
        {
            get
            {
                return (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel);
            }
            set
            {
                panelState.RightPanel = (value) ? PanelState.RightPanelState.PropertyPanel : PanelState.RightPanelState.None;
                OnPropertyChanged();
            }
        }

        public bool ViewSettingBtnIsChecked
        {
            get
            {
                return (panelState.RightPanel == PanelState.RightPanelState.ViewSettings);
            }
            set
            {
                panelState.RightPanel = (value) ? PanelState.RightPanelState.ViewSettings : PanelState.RightPanelState.None;
                OnPropertyChanged();
            }
        }
        #endregion
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region Load document

        private void LoadDefaultDocument()
        {
            string defaultFilePath = "ComPDFKit_Annotations_Sample_File.pdf";
            pdfViewer.PDFView.InitDocument(defaultFilePath);
            LoadDocument();
        }

        private void LoadDocument()
        {
            pdfViewer.PDFView.Load();
            pdfViewer.PDFView.SetShowLink(true);
            
            pdfViewer.PDFView.InfoChanged -= PdfViewer_InfoChanged;
            pdfViewer.PDFView.InfoChanged += PdfViewer_InfoChanged;
            PDFGrid.Child = annotationControl;
            
            annotationControl.PDFViewControl = pdfViewer;
            annotationControl.InitWithPDFViewer(pdfViewer.PDFView);
            annotationControl.ClearAllToolState();
            annotationControl.ExpandRightPropertyPanel(null, Visibility.Collapsed);

            annotationControl.OnCanSaveChanged -= AnnotationControl_OnCanSaveChanged;
            annotationControl.OnCanSaveChanged += AnnotationControl_OnCanSaveChanged;
            annotationControl.OnAnnotEditHandler -= PdfAnnotationControl_RefreshAnnotList;
            annotationControl.OnAnnotEditHandler += PdfAnnotationControl_RefreshAnnotList;
            
            annotationControl.PDFViewControl.PDFView.SetFormFieldHighlight(true);
            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Closed += PasswordUI_Closed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Confirmed += PasswordUI_Confirmed;
            ModeComboBox.SelectedIndex = 1;
            annotationControl.PDFViewControl.PDFView.ChangeFitMode(FitMode.FitWidth);
            CPDFSaclingControl.InitWithPDFViewer(annotationControl.PDFViewControl.PDFView);
            CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)(annotationControl.PDFViewControl.PDFView.ZoomFactor * 100)));

            ViewSettingBtn.IsChecked = false;
            botaBarControl.InitWithPDFViewer(annotationControl.PDFViewControl.PDFView);
            botaBarControl.AddBOTAContent(new []{BOTATools.Thumbnail , BOTATools.Outline , BOTATools.Bookmark , BOTATools.Annotation , BOTATools.Search});
            botaBarControl.SelectBotaTool(BOTATools.Thumbnail);
            annotationControl.SetBOTAContainer(botaBarControl);
            annotationControl.InitialPDFViewControl(annotationControl.PDFViewControl);
            
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
        }
        
        private void OpenFile()
        {
            string filePath = CommonHelper.GetExistedPathOrEmpty();
            if (!string.IsNullOrEmpty(filePath) && annotationControl.PDFViewControl != null)
            {
                if (pdfViewer.PDFView != null && pdfViewer.PDFView.Document != null)
                {
                    string oldFilePath = pdfViewer.PDFView.Document.FilePath;
                    if (oldFilePath.ToLower() == filePath.ToLower())
                    {
                        return;
                    }
                }

                passwordViewer = new PDFViewControl();
                passwordViewer.PDFView.InitDocument(filePath);
                if (passwordViewer.PDFView.Document == null)
                {
                    MessageBox.Show("Open File Failed");
                    return;
                }

                if (passwordViewer.PDFView.Document.IsLocked)
                {
                    PasswordUI.SetShowText(System.IO.Path.GetFileName(filePath) + " " + LanguageHelper.CommonManager.GetString("Tip_Encrypted"));
                    PasswordUI.ClearPassword();
                    PopupBorder.Visibility = Visibility.Visible;
                    PasswordUI.Visibility = Visibility.Visible;
                }
                else
                {
                    pdfViewer.PDFView.Document.Release();
                    pdfViewer = passwordViewer;
                    LoadDocument();
                }
            }
        }

        #endregion
        
        #region Password

        private void PasswordUI_Confirmed(object sender, string e)
        {
            if (passwordViewer != null && passwordViewer.PDFView != null && passwordViewer.PDFView.Document != null)
            {
                passwordViewer.PDFView.Document.UnlockWithPassword(e);
                if (passwordViewer.PDFView.Document.IsLocked == false)
                {
                    PasswordUI.SetShowError("", Visibility.Collapsed);
                    PasswordUI.ClearPassword();
                    PasswordUI.Visibility = Visibility.Collapsed;
                    PopupBorder.Visibility = Visibility.Collapsed;
                    pdfViewer = passwordViewer;
                    LoadDocument();
                }
                else
                {
                    PasswordUI.SetShowError("Wrong Password", Visibility.Visible);
                }
            }
        }

        private void PasswordUI_Canceled(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
            PasswordUI.Visibility = Visibility.Collapsed;
        }

        private void PasswordUI_Closed(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
            PasswordUI.Visibility = Visibility.Collapsed;
        }

        #endregion
        
        #region Load Unload custom control

        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            pdfViewer = new PDFViewControl();
            LoadDefaultDocument();
        }
        
        private void LoadCustomControl()
        {
            regularViewerControl.PdfViewControl = pdfViewer;
            regularViewerControl.InitWithPDFViewer(pdfViewer.PDFView);
            regularViewerControl.PdfViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
            regularViewerControl.SetBOTAContainer(null);
            regularViewerControl.SetBOTAContainer(botaBarControl);
            regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
            PDFGrid.Child = regularViewerControl;

        }
        #endregion
        
        #region Event handle

        private void PdfViewer_InfoChanged(object sender, KeyValuePair<string, object> e)
        { 
            if (e.Key == "Zoom")
            {
                CPDFSaclingControl.SetZoomTextBoxText(string.Format("{0}", (int)((double)e.Value * 100)));
            }
        }
        
        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RightPanel")
            {
                OnPropertyChanged(nameof(RightToolPanelButtonIsChecked));
                OnPropertyChanged(nameof(ViewSettingBtnIsChecked));
            }
        }
        
        private void AnnotationControl_OnCanSaveChanged(object sender, bool e)
        {
            this.CanSave = e;
        }

        private void PdfAnnotationControl_RefreshAnnotList(object sender, EventArgs e)
        {
            botaBarControl.LoadAnnotationList();
        }
        
        private void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
            pdfViewer.PDFView.UndoManager.CanSave = false;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void LeftToolPanelButton_Click(object sender, RoutedEventArgs e)
        {
            panelState.IsLeftPanelExpand = (sender as ToggleButton).IsChecked == true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (item.Content as string == currentMode)
            {
                return;
            }
            ClearPanelState();
            
            if (currentMode == "Viewer")
            {
                regularViewerControl.ClearViewerControl();
            }
            else if (currentMode == "Annotation")
            {
                annotationControl.ClearViewerControl();
            }

            if (item.Content as string == "Viewer")
            {
                if (regularViewerControl.PdfViewControl != null && regularViewerControl.PdfViewControl.PDFView != null)
                {
                    PDFGrid.Child = regularViewerControl;
                    regularViewerControl.PdfViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
                    regularViewerControl.PdfViewControl = pdfViewer;
                    regularViewerControl.InitWithPDFViewer(pdfViewer.PDFView);
                    regularViewerControl.SetBOTAContainer(botaBarControl);
                    regularViewerControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            else if (item.Content as string == "Annotation")
            {
                if (annotationControl.PDFViewControl != null && annotationControl.PDFViewControl.PDFView != null)
                {
                    PDFGrid.Child = annotationControl;
                    annotationControl.PDFViewControl.PDFView.SetMouseMode(MouseModes.Viewer);
                    annotationControl.PDFViewControl = pdfViewer;
                    annotationControl.InitWithPDFViewer(pdfViewer.PDFView);
                    annotationControl.SetBOTAContainer(botaBarControl);
                    annotationControl.SetDisplaySettingsControl(displaySettingsControl);
                }
            }
            currentMode = item.Content as string;
        }

        private void PageInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            PasswordUI.Visibility = Visibility.Collapsed;
            FileInfoUI.Visibility = Visibility.Visible;
            FileInfoControl.InitWithPDFViewer(pdfViewer.PDFView);
            PopupBorder.Visibility = Visibility.Visible;
        }

        private void ViewSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            panelState.RightPanel =
                ((sender as ToggleButton).IsChecked == true) ?
                    PanelState.RightPanelState.ViewSettings : PanelState.RightPanelState.None;
        }

        private void RightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            panelState.RightPanel =
                ((sender as ToggleButton).IsChecked == true) ?
                    PanelState.RightPanelState.PropertyPanel : PanelState.RightPanelState.None;
        }
        
        private void ClearPanelState()
        {
            LeftToolPanelButtonIsChecked = false;
            ViewSettingBtnIsChecked = false;
            RightToolPanelButtonIsChecked = false;
        }

        private void ExpandSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LeftToolPanelButton.IsChecked = true;
            LeftToolPanelButtonIsChecked = true;
            botaBarControl.SelectBotaTool(BOTATools.Search);
        }
        
        private void FileInfoCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion

        #region Save file
        /// <summary>
        /// Save the file to another PDF file.
        /// </summary>
        public void SaveAsFile()
        {
            {
                if (pdfViewer != null && pdfViewer.PDFView != null && pdfViewer.PDFView.Document != null)
                {
                    CPDFDocument pdfDoc = pdfViewer.PDFView.Document;
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "(*.pdf)|*.pdf";
                    saveDialog.DefaultExt = ".pdf";
                    saveDialog.OverwritePrompt = true;

                    if (saveDialog.ShowDialog() == true)
                    {
                        pdfDoc.WriteToFilePath(saveDialog.FileName);
                    }
                }
            }
        }

        /// <summary>
        /// Save the file in the current path.
        /// </summary>
        private void SaveFile()
        {
            if (pdfViewer != null && pdfViewer.PDFView != null && pdfViewer.PDFView.Document != null)
            {
                try
                {
                    CPDFDocument pdfDoc = pdfViewer.PDFView.Document;
                    if (pdfDoc.WriteToLoadedPath())
                    {
                        return;
                    }

                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filter = "(*.pdf)|*.pdf";
                    saveDialog.DefaultExt = ".pdf";
                    saveDialog.OverwritePrompt = true;

                    if (saveDialog.ShowDialog() == true)
                    {
                        pdfDoc.WriteToFilePath(saveDialog.FileName);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        #endregion
    }
}