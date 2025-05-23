using Kinowood.Models;
using Kinowood.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.Core;

namespace Kinowood
{
    public sealed partial class MovieDetailPage : Page
    {
        private DetailedMovie _movie;
        private MovieService _movieService;
        private string _currentVideoUrl;

        public MovieDetailPage()
        {
            this.InitializeComponent();
            _movieService = new MovieService();

            // Register for back button press
            SystemNavigationManager.GetForCurrentView().BackRequested += MovieDetailPage_BackRequested;
        }

        private void MovieDetailPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.GoBack();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            // Unregister from back button press
            SystemNavigationManager.GetForCurrentView().BackRequested -= MovieDetailPage_BackRequested;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string movieId = e.Parameter as string;
            if (movieId != null)
            {
                try
                {
                    var response = await _movieService.GetDetailedMovieAsync(movieId);
                    if (response != null && response.Film != null)
                    {
                        _movie = response.Film;
                        DisplayMovieDetails();
                        SetupVideoPlayer();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to load movie details: Response or Film is null");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading movie details: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        private void DisplayMovieDetails()
        {
            if (_movie == null)
            {
                System.Diagnostics.Debug.WriteLine("DisplayMovieDetails: _movie is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Displaying movie details:");
            System.Diagnostics.Debug.WriteLine($"Title: {_movie.Title}");
            System.Diagnostics.Debug.WriteLine($"Original Title: {_movie.OriginalTitle}");
            System.Diagnostics.Debug.WriteLine($"Year: {_movie.Year}");
            System.Diagnostics.Debug.WriteLine($"Description: {_movie.Description}");
            System.Diagnostics.Debug.WriteLine($"Director: {_movie.Director}");
            System.Diagnostics.Debug.WriteLine($"Writer: {_movie.Writer}");
            System.Diagnostics.Debug.WriteLine($"Actors: {_movie.Actors}");
            System.Diagnostics.Debug.WriteLine($"Genres: {_movie.Genres}");
            System.Diagnostics.Debug.WriteLine($"Duration: {_movie.Duration}");
            System.Diagnostics.Debug.WriteLine($"Poster: {_movie.Poster}");

            try
            {
                if (TitleText != null) TitleText.Text = _movie.Title ?? "N/A";
                if (OriginalTitleText != null) OriginalTitleText.Text = _movie.OriginalTitle ?? "N/A";
                if (YearText != null) YearText.Text = _movie.Year ?? "N/A";
                if (DescriptionText != null) DescriptionText.Text = _movie.Description ?? "N/A";
                if (DirectorText != null) DirectorText.Text = "Director: " + (_movie.Director ?? "N/A");
                if (WriterText != null) WriterText.Text = "Writer: " + (_movie.Writer ?? "N/A");
                if (ActorsText != null) ActorsText.Text = "Actors: " + (_movie.Actors ?? "N/A");
                if (GenresText != null) GenresText.Text = "Genres: " + (_movie.Genres ?? "N/A");
                if (DurationText != null) DurationText.Text = "Duration: " + (_movie.Duration ?? "N/A");

                if (!string.IsNullOrEmpty(_movie.Poster) && PosterImage != null)
                {
                    var posterUrl = Config.ApiBaseUrl + _movie.Poster;
                    System.Diagnostics.Debug.WriteLine($"Loading poster from: {posterUrl}");
                    PosterImage.Source = new BitmapImage(new Uri(posterUrl));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DisplayMovieDetails: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"Error source: {ex.Source}");
            }
        }

        private Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(query)) return result;

            // Remove the leading '?' if present
            if (query.StartsWith("?")) query = query.Substring(1);

            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    result[parts[0]] = Uri.UnescapeDataString(parts[1]);
                }
            }
            return result;
        }

        private void SetupVideoPlayer()
        {
            if (_movie?.Video == null)
            {
                System.Diagnostics.Debug.WriteLine("SetupVideoPlayer: Video information is null");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("Setting up video player with URLs:");
                foreach (var url in _movie.Video.Urls)
                {
                    System.Diagnostics.Debug.WriteLine($"Quality {url.Key}: {url.Value}");
                }

                // Setup quality selection
                var qualities = _movie.Video.Urls.Select(kvp => new
                {
                    Quality = kvp.Key,
                    Name = kvp.Value
                }).ToList();

                QualityComboBox.ItemsSource = qualities;
                QualityComboBox.DisplayMemberPath = "Name";
                QualityComboBox.SelectedValuePath = "Quality";

                // Set default quality (720p if available, otherwise first available)
                var defaultQuality = "720";
                if (!_movie.Video.Urls.ContainsKey(defaultQuality))
                {
                    defaultQuality = _movie.Video.Urls.Keys.FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(defaultQuality))
                {
                    QualityComboBox.SelectedValue = defaultQuality;
                    
                    // Extract group_id and video_id from the URL
                    var videoUrl = _movie.Video.Urls[defaultQuality];
                    var uri = new Uri(videoUrl);
                    var query = ParseQueryString(uri.Query);
                    
                    var groupId = query["group_id"];
                    var videoId = query["video_id"];
                    
                    if (!string.IsNullOrEmpty(groupId) && !string.IsNullOrEmpty(videoId))
                    {
                        var proxyUrl = $"{Config.ApiBaseUrl}proxy.php?group_id={groupId}&video_id={videoId}&quality={defaultQuality}";
                        System.Diagnostics.Debug.WriteLine($"Setting up video player with proxy URL: {proxyUrl}");
                        _currentVideoUrl = proxyUrl;
                        VideoPlayer.Source = new Uri(proxyUrl);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Could not extract group_id or video_id from URL");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No quality URLs available");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SetupVideoPlayer: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void QualityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QualityComboBox.SelectedValue != null && _movie?.Video != null)
            {
                try
                {
                    var quality = QualityComboBox.SelectedValue.ToString();
                    if (_movie.Video.Urls.ContainsKey(quality))
                    {
                        var videoUrl = _movie.Video.Urls[quality];
                        var uri = new Uri(videoUrl);
                        var query = ParseQueryString(uri.Query);
                        
                        var groupId = query["group_id"];
                        var videoId = query["video_id"];
                        
                        if (!string.IsNullOrEmpty(groupId) && !string.IsNullOrEmpty(videoId))
                        {
                            var proxyUrl = $"{Config.ApiBaseUrl}proxy.php?group_id={groupId}&video_id={videoId}&quality={quality}";
                            System.Diagnostics.Debug.WriteLine($"Changing video quality to: {quality}");
                            System.Diagnostics.Debug.WriteLine($"New proxy URL: {proxyUrl}");
                            _currentVideoUrl = proxyUrl;
                            VideoPlayer.Source = new Uri(proxyUrl);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Could not extract group_id or video_id from URL");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error changing video quality: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        private void VideoPlayer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(VideoPlayer);
            var width = VideoPlayer.ActualWidth;
            
            if (position.X < width / 2)
            {
                // Double tap on left side - rewind 10 seconds
                VideoPlayer.Position = VideoPlayer.Position.Subtract(TimeSpan.FromSeconds(10));
            }
            else
            {
                // Double tap on right side - forward 10 seconds
                VideoPlayer.Position = VideoPlayer.Position.Add(TimeSpan.FromSeconds(10));
            }
        }
    }
} 