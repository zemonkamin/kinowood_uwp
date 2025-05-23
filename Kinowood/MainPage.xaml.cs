using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.System;
using Kinowood.Models;
using Kinowood.Services;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Kinowood
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MovieService _movieService;
        private ObservableCollection<Movie> _movies;
        private ContentDialog _errorDialog;

        public MainPage()
        {
            this.InitializeComponent();
            _movieService = new MovieService();
            _movies = new ObservableCollection<Movie>();
            MoviesGrid.ItemsSource = _movies;
            _errorDialog = new ContentDialog
            {
                Title = "Error",
                PrimaryButtonText = "OK"
            };

            // Load movies when the page loads
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMovies();
        }

        private async System.Threading.Tasks.Task LoadMovies()
        {
            try
            {
                LoadingIndicator.IsActive = true;
                _movies.Clear();

                // Show logo and hide search results initially
                LogoImage.Visibility = Visibility.Visible;
                SearchGrid.Visibility = Visibility.Visible;
                MoviesGrid.Visibility = Visibility.Visible; // Ensure movie grid is visible

                System.Diagnostics.Debug.WriteLine("Loading initial movies");
                var response = await _movieService.GetMoviesAsync();
                
                if (response == null)
                {
                    System.Diagnostics.Debug.WriteLine("Response is null");
                    await ShowError("No response from server");
                    return;
                }

                if (response.Films != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Adding {response.Films.Count} movies to the grid");
                    foreach (var movie in response.Films)
                    {
                        _movies.Add(movie);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadMovies: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await ShowError($"Failed to load movies: {ex.Message}");
            }
            finally
            {
                LoadingIndicator.IsActive = false;
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await SearchMovies();
        }

        private async void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                await SearchMovies();
            }
        }

        private async System.Threading.Tasks.Task SearchMovies()
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                // If search box is empty, show logo and hide search results
                LogoImage.Visibility = Visibility.Visible;
                MoviesGrid.Visibility = Visibility.Visible; // Show initial movies if needed
                // SearchGrid.Visibility = Visibility.Visible; // Keep search box visible
                await LoadMovies(); // Reload initial movies
                return;
            }

            try
            {
                // Hide logo and show search results
                LogoImage.Visibility = Visibility.Collapsed;
                MoviesGrid.Visibility = Visibility.Visible; // Ensure movie grid is visible

                LoadingIndicator.IsActive = true;
                _movies.Clear();

                System.Diagnostics.Debug.WriteLine($"Starting search for: {SearchBox.Text}");
                var response = await _movieService.SearchMoviesAsync(SearchBox.Text);
                
                if (response == null)
                {
                    System.Diagnostics.Debug.WriteLine("Response is null");
                    await ShowError("No response from server");
                    return;
                }

                if (response.Films != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Adding {response.Films.Count} movies to the grid");
                    foreach (var movie in response.Films)
                    {
                        _movies.Add(movie);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SearchMovies: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await ShowError($"Failed to search movies: {ex.Message}");
            }
            finally
            {
                LoadingIndicator.IsActive = false;
            }
        }

        private async System.Threading.Tasks.Task ShowError(string message)
        {
            try
            {
                _errorDialog.Content = message;
                await _errorDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var image = sender as Image;
            var movie = image?.DataContext as Movie;
            System.Diagnostics.Debug.WriteLine($"Failed to load image for movie {movie?.Title}: {e.ErrorMessage}");
            System.Diagnostics.Debug.WriteLine($"Image URL: {movie?.FullImagePath}");
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            var movie = image?.DataContext as Movie;
            System.Diagnostics.Debug.WriteLine($"Successfully loaded image for movie {movie?.Title}");
            System.Diagnostics.Debug.WriteLine($"Image URL: {movie?.FullImagePath}");
        }

        private void MoviesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedMovie = e.ClickedItem as Movie;
            if (clickedMovie != null)
            {
                System.Diagnostics.Debug.WriteLine($"Clicked movie: {clickedMovie.Title}, ID: {clickedMovie.Id}");
                try
                {
                    Frame.Navigate(typeof(MovieDetailPage), clickedMovie.Id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Clicked item is not a Movie");
            }
        }

        private void MovieCard_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var movie = grid?.DataContext as Movie;
            if (movie != null)
            {
                System.Diagnostics.Debug.WriteLine($"Tapped movie: {movie.Title}, ID: {movie.Id}");
                try
                {
                    Frame.Navigate(typeof(MovieDetailPage), movie.Id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Tapped item is not a Movie");
            }
        }
    }
}

