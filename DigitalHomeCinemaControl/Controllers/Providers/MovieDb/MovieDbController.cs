/*
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
 * more details.
 *
 */

namespace DigitalHomeCinemaControl.Controllers.Providers.MovieDb
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Controllers.Base;
    using TMDbLib.Client;
    using TMDbLib.Objects.General;
    using TMDbLib.Objects.Search;

    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "<Pending>")]
    public class MovieDbController : DeviceController, IMediaInfoController
    {

        #region Members

        private const string POSTER_URL = "https://image.tmdb.org/t/p/w600_and_h900_bestv2";
        private TMDbClient movieApi;

        #endregion

        #region Constructor

        public MovieDbController()
            : base()
        {
            this.ApiKey = string.Empty;

            this.DataSource = new DispatchedBindingList<IBindingItem> {
                RaiseListChangedEvents = true
            };

            this.DataSource.Add(new BindingItem<string>("PosterPath"));
            this.DataSource.Add(new BindingItem<string>("Description"));
        }

        #endregion

        #region Methods

        public void GetFeatureInfo(string title, string year = "")
        {
            if (string.IsNullOrEmpty(title)) {
                UpdateDataSource<string>("PosterPath", string.Empty);
                UpdateDataSource<string>("Description", string.Empty);
                return;
            }

            try {
                SearchContainer<SearchMovie> results = this.movieApi.SearchMovieAsync(title).Result;

                if (results.Results.Count == 0) {
                    OnError(string.Format(CultureInfo.InvariantCulture, "TMDB failed to find result for {0}", title));
                    UpdateDataSource<string>("PosterPath", string.Empty);
                    UpdateDataSource<string>("Description", string.Empty);
                    return;
                }

                foreach (var result in results.Results) {
                    if (string.IsNullOrEmpty(year) && (!string.IsNullOrEmpty(result.PosterPath))) {
                        UpdateDataSource<string>("PosterPath", POSTER_URL + result.PosterPath);
                        UpdateDataSource<string>("Description", result.Overview);
                        break;
                    } else if ((title == result.Title) && (result.ReleaseDate?.Year.ToString(CultureInfo.InvariantCulture) == year) &&
                        (!string.IsNullOrEmpty(result.PosterPath))) {
                        UpdateDataSource<string>("PosterPath", POSTER_URL + result.PosterPath);
                        UpdateDataSource<string>("Description", result.Overview);
                        break;
                    }
                }
            } catch {
                OnError(string.Format(CultureInfo.InvariantCulture, "TMDB failed to find result for {0}", title));
                UpdateDataSource<string>("PosterPath", string.Empty);
                UpdateDataSource<string>("Description", string.Empty);
            }
        }

        public override void Connect()
        {
            if (this.movieApi != null) { return; }

            if (string.IsNullOrEmpty(this.ApiKey)) {
                OnError(Properties.Resources.MSG_TMDB_API_KEY_ERROR);
            } else {
                this.movieApi = new TMDbClient(this.ApiKey, false);
                OnConnected();
            }
        }

        public override void Disconnect()
        {
            if (this.movieApi != null) {
                this.movieApi.Dispose();
                this.movieApi = null;
            }
            OnDisconnected();
        }

        #endregion

        #region Properties

        public string ApiKey
        {
            get { return GetSetting<string>(); }
            set { Setting<string>(value); }
        }

        #endregion

    }

}
