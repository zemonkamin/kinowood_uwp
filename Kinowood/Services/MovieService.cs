using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kinowood.Models;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;

namespace Kinowood.Services
{
    public class MovieService
    {
        private readonly HttpClient _httpClient;
        private HashSet<string> _processedMovieIds;

        public MovieService()
        {
            _httpClient = new HttpClient();
            _processedMovieIds = new HashSet<string>();
        }

        private List<Movie> GetUniqueMovies(List<Movie> movies)
        {
            if (movies == null) return new List<Movie>();
            
            var uniqueMovies = new List<Movie>();
            foreach (var movie in movies)
            {
                if (!_processedMovieIds.Contains(movie.Id))
                {
                    _processedMovieIds.Add(movie.Id);
                    uniqueMovies.Add(movie);
                }
            }
            return uniqueMovies;
        }

        private List<Movie> GetAllMovies(ApiResponse response)
        {
            var allMovies = new List<Movie>();

            // Add movies from Films if available
            if (response.Films != null)
            {
                allMovies.AddRange(GetUniqueMovies(response.Films));
            }

            // Add movies from NoPosterFilms if available
            if (response.NoPosterFilms != null)
            {
                allMovies.AddRange(GetUniqueMovies(response.NoPosterFilms));
            }

            // Add movies from GenreFilms if available
            if (response.GenreFilms != null)
            {
                foreach (var genreMovies in response.GenreFilms.Values)
                {
                    allMovies.AddRange(GetUniqueMovies(genreMovies));
                }
            }

            return allMovies;
        }

        public async Task<ApiResponse> GetMoviesAsync()
        {
            try
            {
                _processedMovieIds.Clear();
                var url = $"{Config.ApiBaseUrl}{Config.ApiEndpoint}";
                System.Diagnostics.Debug.WriteLine($"Fetching movies with URL: {url}");
                
                var response = await _httpClient.GetStringAsync(url);
                System.Diagnostics.Debug.WriteLine($"Raw API Response: {response}");
                
                if (string.IsNullOrEmpty(response))
                {
                    System.Diagnostics.Debug.WriteLine("API returned empty response");
                    return null;
                }

                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        Error = (sender, args) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"JSON parsing error: {args.ErrorContext.Error.Message}");
                            System.Diagnostics.Debug.WriteLine($"Error path: {args.ErrorContext.Path}");
                            System.Diagnostics.Debug.WriteLine($"Error member: {args.ErrorContext.Member}");
                            args.ErrorContext.Handled = true;
                        }
                    };

                    var result = JsonConvert.DeserializeObject<ApiResponse>(response, settings);
                    if (result == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to deserialize response");
                        return null;
                    }

                    // Log the deserialized object structure
                    System.Diagnostics.Debug.WriteLine("Deserialized object structure:");
                    System.Diagnostics.Debug.WriteLine($"Films: {(result.Films != null ? result.Films.Count.ToString() : "null")}");
                    System.Diagnostics.Debug.WriteLine($"NoPosterFilms: {(result.NoPosterFilms != null ? result.NoPosterFilms.Count.ToString() : "null")}");
                    System.Diagnostics.Debug.WriteLine($"GenreFilms: {(result.GenreFilms != null ? result.GenreFilms.Count.ToString() : "null")}");
                    System.Diagnostics.Debug.WriteLine($"Total: {result.Total}");
                    System.Diagnostics.Debug.WriteLine($"TotalPages: {result.TotalPages}");
                    System.Diagnostics.Debug.WriteLine($"CurrentPage: {result.CurrentPage}");
                    System.Diagnostics.Debug.WriteLine($"PerPage: {result.PerPage}");
                    System.Diagnostics.Debug.WriteLine($"Film: {(result.Film != null ? "not null" : "null")}");
                    System.Diagnostics.Debug.WriteLine($"Similar: {(result.Similar != null ? "not null" : "null")}");

                    // Get all unique movies from all available sources
                    var allMovies = GetAllMovies(result);
                    result.Films = allMovies;

                    System.Diagnostics.Debug.WriteLine($"Found {allMovies.Count} unique movies in total");
                    foreach (var movie in allMovies)
                    {
                        System.Diagnostics.Debug.WriteLine($"Movie: {movie.Title} ({movie.Year})");
                        System.Diagnostics.Debug.WriteLine($"Image URL: {movie.FullImagePath}");
                        System.Diagnostics.Debug.WriteLine($"Poster URL: {movie.FullPosterPath}");
                    }
                    
                    return result;
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"JSON parsing error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"JSON content: {response}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching movies: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ApiResponse> SearchMoviesAsync(string query)
        {
            try
            {
                _processedMovieIds.Clear();
                var url = Config.GetSearchUrl(query);
                System.Diagnostics.Debug.WriteLine($"Searching movies with URL: {url}");
                
                var response = await _httpClient.GetStringAsync(url);
                System.Diagnostics.Debug.WriteLine($"Raw API Response: {response}");
                
                if (string.IsNullOrEmpty(response))
                {
                    System.Diagnostics.Debug.WriteLine("API returned empty response");
                    return null;
                }

                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        Error = (sender, args) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"JSON parsing error: {args.ErrorContext.Error.Message}");
                            System.Diagnostics.Debug.WriteLine($"Error path: {args.ErrorContext.Path}");
                            System.Diagnostics.Debug.WriteLine($"Error member: {args.ErrorContext.Member}");
                            args.ErrorContext.Handled = true;
                        }
                    };

                    var result = JsonConvert.DeserializeObject<ApiResponse>(response, settings);
                    if (result == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to deserialize response");
                        return null;
                    }

                    // Log the deserialized object structure
                    System.Diagnostics.Debug.WriteLine("Deserialized object structure:");
                    System.Diagnostics.Debug.WriteLine($"Films: {(result.Films != null ? result.Films.Count.ToString() : "null")}");
                    System.Diagnostics.Debug.WriteLine($"NoPosterFilms: {(result.NoPosterFilms != null ? result.NoPosterFilms.Count.ToString() : "null")}");
                    System.Diagnostics.Debug.WriteLine($"GenreFilms: {(result.GenreFilms != null ? result.GenreFilms.Count.ToString() : "null")}");
                    System.Diagnostics.Debug.WriteLine($"Total: {result.Total}");
                    System.Diagnostics.Debug.WriteLine($"TotalPages: {result.TotalPages}");
                    System.Diagnostics.Debug.WriteLine($"CurrentPage: {result.CurrentPage}");
                    System.Diagnostics.Debug.WriteLine($"PerPage: {result.PerPage}");
                    System.Diagnostics.Debug.WriteLine($"Film: {(result.Film != null ? "not null" : "null")}");
                    System.Diagnostics.Debug.WriteLine($"Similar: {(result.Similar != null ? "not null" : "null")}");

                    // Get all unique movies from all available sources
                    var allMovies = GetAllMovies(result);
                    result.Films = allMovies;

                    System.Diagnostics.Debug.WriteLine($"Found {allMovies.Count} unique movies in total");
                    foreach (var movie in allMovies)
                    {
                        System.Diagnostics.Debug.WriteLine($"Movie: {movie.Title} ({movie.Year})");
                        System.Diagnostics.Debug.WriteLine($"Image URL: {movie.FullImagePath}");
                        System.Diagnostics.Debug.WriteLine($"Poster URL: {movie.FullPosterPath}");
                    }
                    
                    return result;
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"JSON parsing error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"JSON content: {response}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching movies: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<DetailedMovieResponse> GetDetailedMovieAsync(string movieId)
        {
            try
            {
                var url = $"{Config.ApiBaseUrl}api/film.php?id={movieId}";
                System.Diagnostics.Debug.WriteLine($"Fetching detailed movie info from: {url}");
                System.Diagnostics.Debug.WriteLine($"Full API URL: {url}");

                using (var client = new HttpClient())
                {
                    // Add User-Agent header to avoid PHP notices
                    client.DefaultRequestHeaders.Add("User-Agent", "Kinowood UWP App");
                    
                    var response = await client.GetStringAsync(url);
                    System.Diagnostics.Debug.WriteLine($"Raw response: {response}");
                    
                    // Extract JSON from response by finding the first '{' and last '}'
                    int startIndex = response.IndexOf('{');
                    int endIndex = response.LastIndexOf('}');
                    
                    if (startIndex >= 0 && endIndex > startIndex)
                    {
                        string jsonContent = response.Substring(startIndex, endIndex - startIndex + 1);
                        System.Diagnostics.Debug.WriteLine($"Extracted JSON: {jsonContent}");
                        
                        var settings = new JsonSerializerSettings
                        {
                            Error = (sender, args) =>
                            {
                                System.Diagnostics.Debug.WriteLine($"JSON parsing error: {args.ErrorContext.Error.Message}");
                                System.Diagnostics.Debug.WriteLine($"Error path: {args.ErrorContext.Path}");
                                System.Diagnostics.Debug.WriteLine($"Error member: {args.ErrorContext.Member}");
                                args.ErrorContext.Handled = true;
                            }
                        };

                        var result = JsonConvert.DeserializeObject<DetailedMovieResponse>(jsonContent, settings);
                        
                        if (result == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to deserialize response");
                            return null;
                        }

                        // Log the deserialized object structure
                        System.Diagnostics.Debug.WriteLine("Deserialized object structure:");
                        System.Diagnostics.Debug.WriteLine($"Film: {(result.Film != null ? "not null" : "null")}");
                        System.Diagnostics.Debug.WriteLine($"Similar: {(result.Similar != null ? "not null" : "null")}");
                        
                        if (result.Film != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Film details:");
                            System.Diagnostics.Debug.WriteLine($"Title: {result.Film.Title}");
                            System.Diagnostics.Debug.WriteLine($"Year: {result.Film.Year}");
                            System.Diagnostics.Debug.WriteLine($"Video: {(result.Film.Video != null ? "not null" : "null")}");
                            if (result.Film.Video != null)
                            {
                                // Replace kinowood.php with proxy.php in video URLs
                                var newUrls = new Dictionary<string, string>();
                                foreach (var kvp in result.Film.Video.Urls)
                                {
                                    var newUrl = kvp.Value.Replace("kinowood.php", "proxy.php");
                                    newUrls[kvp.Key] = newUrl;
                                }
                                result.Film.Video.Urls = newUrls;
                                
                                System.Diagnostics.Debug.WriteLine($"Video URLs: {string.Join(", ", result.Film.Video.Urls?.Select(k => $"{k.Key}: {k.Value}") ?? new string[0])}");
                            }
                        }
                        
                        return result;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No valid JSON found in response");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching detailed movie: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
} 