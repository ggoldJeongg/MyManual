using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MyManual.Views
{
    public partial class ImageViewerWindow : Window
    {
        private static readonly HttpClient _httpClient = new();

        public ImageViewerWindow(string imageUrl, string fileName)
        {
            InitializeComponent();

            FileNameText.Text = fileName;
            LoadImageAsync(imageUrl);
        }

        private async void LoadImageAsync(string imageUrl)
        {
            try
            {
                // HTTP로 이미지 다운로드
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);

                // UI 스레드에서 BitmapImage 생성
                await Dispatcher.InvokeAsync(() =>
                {
                    var bitmap = new BitmapImage();
                    using (var stream = new MemoryStream(imageBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }
                    bitmap.Freeze();

                    ImageViewer.Source = bitmap;

                    // 창 크기를 이미지 크기에 맞춤 (화면 크기 제한)
                    var screenWidth = SystemParameters.WorkArea.Width;
                    var screenHeight = SystemParameters.WorkArea.Height;

                    var imageWidth = bitmap.PixelWidth + 40; // 여백 포함
                    var imageHeight = bitmap.PixelHeight + 80; // 여백 + 상단 버튼 공간

                    // 화면의 90%를 넘지 않도록 제한
                    Width = Math.Min(imageWidth, screenWidth * 0.9);
                    Height = Math.Min(imageHeight, screenHeight * 0.9);

                    // 최소 크기 보장
                    if (Width < 400) Width = 400;
                    if (Height < 300) Height = 300;

                    // 로딩 숨김
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageViewer] 이미지 로드 실패: {ex.Message}");
                Dispatcher.Invoke(() =>
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    MessageBox.Show("이미지를 불러올 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                });
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // ESC 키로 닫기
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
