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
    using System;
    using System.Globalization;
    using DigitalHomeCinemaControl.Collections;
    using DigitalHomeCinemaControl.Components;
    using DigitalHomeCinemaControl.Controllers.Base;
    using TMDbLib.Client;
    using TMDbLib.Objects.General;
    using TMDbLib.Objects.Search;

    public sealed class MovieDbController : DeviceController, IMediaInfoController, IDisposable
    {

        #region Members

        private const string POSTERPATH = "PosterPath";
        private const string DESCRIPTION = "Description";

        private const string POSTER_URL = "https://image.tmdb.org/t/p/w600_and_h900_bestv2";
        private TMDbClient movieApi;
        private volatile bool disposed;

        #endregion

        #region Constructor

        public MovieDbController()
            : base()
        {
            this.ApiKey = string.Empty;

            this.DataSource = new DispatchedBindingList<IBindingItem> {
                RaiseListChangedEvents = true
            };

            this.DataSource.Add(new BindingItem<string>(POSTERPATH));
            this.DataSource.Add(new BindingItem<string>(DESCRIPTION));
        }

        #endregion

        #region Methods

        public override void Connect()
        {
            if (string.IsNullOrEmpty(this.ApiKey)) {
                this.ControllerStatus = ControllerStatus.Error;
                OnError(Properties.Resources.MSG_TMDB_API_KEY_ERROR);
                return;
            }

            this.disposed = false;

            try {
                this.movieApi = new TMDbClient(this.ApiKey, false);
                OnConnected();
            } catch {
                this.ControllerStatus = ControllerStatus.Error;
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_NETWORK_TIMEOUT, "TMDB"));
            }  
        }

        public override void Disconnect()
        {
            try {
                Dispose(true);
            } catch {
            } finally {
                OnDisconnected();
            }            
        }

        public void GetFeatureInfo(string title, string year = "")
        {
            if (string.IsNullOrEmpty(title)) {
                UpdateDataSource<string>(POSTERPATH, string.Empty);
                UpdateDataSource<string>(DESCRIPTION, string.Empty);
                return;
            }

            try {
                SearchContainer<SearchMovie> results = this.movieApi.SearchMovieAsync(title).Result;

                if (results.Results.Count == 0) {
                    OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_TMDB_NO_RESULT, title));
                    UpdateDataSource<string>(POSTERPATH, string.Empty);
                    UpdateDataSource<string>(DESCRIPTION, string.Empty);
                    return;
                }

                foreach (var result in results.Results) {
                    if (string.IsNullOrEmpty(year) && (!string.IsNullOrEmpty(result.PosterPath))) {
                        UpdateDataSource<string>(POSTERPATH, POSTER_URL + result.PosterPath);
                        UpdateDataSource<string>(DESCRIPTION, result.Overview);
                        break;
                    } else if ((title == result.Title) && (result.ReleaseDate?.Year.ToString(CultureInfo.InvariantCulture) == year) &&
                        (!string.IsNullOrEmpty(result.PosterPath))) {
                        UpdateDataSource<string>(POSTERPATH, POSTER_URL + result.PosterPath);
                        UpdateDataSource<string>(DESCRIPTION, result.Overview);
                        break;
                    }
                }
            } catch {
                OnError(string.Format(CultureInfo.InvariantCulture, Properties.Resources.FMT_TMDB_NO_RESULT, title));
                UpdateDataSource<string>(POSTERPATH, string.Empty);
                UpdateDataSource<string>(DESCRIPTION, string.Empty);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed) {
                if (disposing) {
                    this.movieApi?.Dispose();
                }

                this.disposed = true;

                this.movieApi = null;
            }
        }

        ~MovieDbController()
        {
            Dispose(false);
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#pragma warning restore CA1063

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
