﻿using RainbowMage.OverlayPlugin.Overlays;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RainbowMage.OverlayPlugin
{
    [Serializable]
    public class PluginConfig
    {
        #region Config for version 0.1.2.0 or below
#pragma warning disable 612, 618
        [Obsolete] public event EventHandler<VisibleStateChangedEventArgs> VisibleChanged;
        [Obsolete] public event EventHandler<ThruStateChangedEventArgs> ClickThruChanged;
        [Obsolete] public event EventHandler<UrlChangedEventArgs> UrlChanged;
        [Obsolete] public event EventHandler<SortKeyChangedEventArgs> SortKeyChanged;
        [Obsolete] public event EventHandler<SortTypeChangedEventArgs> SortTypeChanged;

        private bool isVisible;
        [XmlElement("IsVisible")]
        public bool IsVisibleObsolete
        { 
            get
            {
                return this.isVisible;
            }
            set
            {
                if (this.isVisible != value)
                {
                    this.isVisible = value;
                    if (VisibleChanged != null)
                    {
                        VisibleChanged(this, new VisibleStateChangedEventArgs(this.isVisible));
                    }
                }
            }
        }

        private bool isClickThru;
        [XmlElement("IsClickThru")]
        public bool IsClickThruObsolete
        {
            get
            {
                return this.isClickThru;
            }
            set
            {
                if (this.isClickThru != value)
                {
                    this.isClickThru = value;
                    if (ClickThruChanged != null)
                    {
                        ClickThruChanged(this, new ThruStateChangedEventArgs(this.isClickThru));
                    }
                }
            }
        }

        private Point overlayPosition;
        [XmlElement("OverlayPosition")]
        public Point OverlayPositionObsolete
        { 
            get
            {
                return this.overlayPosition;
            }
            set
            {
                this.overlayPosition = value;
            }
        }

        private Size overlaySize;
        [XmlElement("OverlaySize")]
        public Size OverlaySizeObsolete
        {
            get
            {
                return this.overlaySize;
            }
            set
            {
                this.overlaySize = value;
            }
        }

        private string url;
        [XmlElement("Url")]
        public string UrlObsolete
        {
            get
            {
                return this.url;
            }
            set
            {
                if (this.url != value)
                {
                    this.url = value;
                    if (UrlChanged != null)
                    {
                        UrlChanged(this, new UrlChangedEventArgs(this.url));
                    }
                }
            }
        }

        private string sortKey;
        [XmlElement("SortKey")]
        public string SortKeyObsolete
        {
            get
            {
                return this.sortKey;
            }
            set
            {
                if (this.sortKey != value)
                {
                    this.sortKey = value;
                    if (SortKeyChanged != null)
                    {
                        SortKeyChanged(this, new SortKeyChangedEventArgs(this.sortKey));
                    }
                }
            }
        }

        private MiniParseSortType sortType;
        [XmlElement("SortType")]
        public MiniParseSortType SortTypeObsolete
        {
            get
            {
                return this.sortType;
            }
            set
            {
                if (this.sortType != value)
                {
                    this.sortType = value;
                    if (SortTypeChanged != null)
                    {
                        SortTypeChanged(this, new SortTypeChangedEventArgs(this.sortType));
                    }
                }
            }
        }
#pragma warning restore 612, 618
        #endregion

        #region Config for version 0.2.5.0 or below
        [XmlElement("MiniParseOverlay")]
        public MiniParseOverlayConfig MiniParseOverlayObsolete { get; set; }

        [XmlElement("SpellTimerOverlay")]
        public SpellTimerOverlayConfig SpellTimerOverlayObsolete { get; set; }
        #endregion

        [XmlElement("Overlays")]
        public List<OverlayConfig> Overlays { get; set; }

        [XmlElement("FollowLatestLog")]
        public bool FollowLatestLog { get; set; }

        [XmlIgnore]
        public Version Version { get; set; }

        [XmlElement("Version")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public string VersionString
        {
            get
            {
                return this.Version.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Version = null;
                }
                else
                {
                    this.Version = new Version(value);
                }
            }
        }

        [XmlIgnore]
        public bool IsFirstLaunch { get; set; }

        internal const string DefaultMiniParseOverlayName = "Mini Parse";
        internal const string DefaultSpellTimerOverlayName = "Spell Timer";

        public PluginConfig()
        {
            #region Config for version 0.1.2.0 or below
#pragma warning disable 612, 618
            this.IsVisibleObsolete = true;
            this.IsClickThruObsolete = false;
            this.OverlayPositionObsolete = new Point(20, 20);
            this.OverlaySizeObsolete = new Size(300, 300);
            this.UrlObsolete = null;
            this.SortKeyObsolete = "encdps";
            this.SortTypeObsolete = MiniParseSortType.NumericDescending;
#pragma warning restore 612, 618
            #endregion

            this.Overlays = new List<OverlayConfig>();

            this.FollowLatestLog = false;
            this.IsFirstLaunch = true;
        }

        public void SaveXml(string path)
        {
            this.Version = typeof(PluginMain).Assembly.GetName().Version;

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PluginConfig), GetExtraTypes());
                serializer.Serialize(stream, this);
            }
        }

        public static PluginConfig LoadXml(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Specified file is not exists.", path);
            }

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PluginConfig), GetExtraTypes());
                var result = (PluginConfig)serializer.Deserialize(stream);

                result.IsFirstLaunch = false;

                if (result.Version == null)
                {
                    result.UpdateFromVersion0_1_2_0OrBelow();
                    result.UpdateFromVersion0_2_5_0OrBelow();
                }


                return result;
            }
        }

        private static Type[] GetExtraTypes()
        {
            return OverlayTypeManager.ConfigToOverlayDict.Keys.ToArray();
        }

        public void SetDefaultOverlayConfigs()
        {
            var miniparseOverlayConfig = new MiniParseOverlayConfig(DefaultMiniParseOverlayName);
            miniparseOverlayConfig.Position = new Point(20, 20);
            miniparseOverlayConfig.Size = new Size(500, 300);

            var spellTimerOverlayConfig = new SpellTimerOverlayConfig(DefaultSpellTimerOverlayName);
            spellTimerOverlayConfig.Position = new Point(20, 520);
            spellTimerOverlayConfig.Size = new Size(200, 300);
            spellTimerOverlayConfig.IsVisible = true;
            spellTimerOverlayConfig.MaxFrameRate = 5;

            this.Overlays = new List<OverlayConfig>();
            this.Overlays.Add(miniparseOverlayConfig);
            this.Overlays.Add(spellTimerOverlayConfig);
        }

        private void UpdateFromVersion0_1_2_0OrBelow()
        {
#pragma warning disable 612, 618
            if (this.MiniParseOverlayObsolete == null)
            {
                this.MiniParseOverlayObsolete = new MiniParseOverlayConfig(DefaultMiniParseOverlayName);
                this.MiniParseOverlayObsolete.IsVisible = this.IsVisibleObsolete;
                this.MiniParseOverlayObsolete.IsClickThru = this.IsClickThruObsolete;
                this.MiniParseOverlayObsolete.Position = this.OverlayPositionObsolete;
                this.MiniParseOverlayObsolete.Size = this.OverlaySizeObsolete;
                this.MiniParseOverlayObsolete.Url = this.UrlObsolete;
            }
            if (this.SpellTimerOverlayObsolete == null)
            {
                this.SpellTimerOverlayObsolete = new SpellTimerOverlayConfig(DefaultSpellTimerOverlayName);
                this.SpellTimerOverlayObsolete.Position = new Point(20, 520);
                this.SpellTimerOverlayObsolete.Size = new Size(200, 300);
                this.SpellTimerOverlayObsolete.IsVisible = false;
                this.SpellTimerOverlayObsolete.MaxFrameRate = 5;
                this.SpellTimerOverlayObsolete.Url = null;
            }
#pragma warning restore 612, 618
        }

        private void UpdateFromVersion0_2_5_0OrBelow()
        {
#pragma warning disable 612, 618
            if (this.MiniParseOverlayObsolete != null)
            {
                var miniParseOverlayConfig = new MiniParseOverlayConfig(DefaultMiniParseOverlayName);
                miniParseOverlayConfig.IsVisible = this.MiniParseOverlayObsolete.IsVisible;
                miniParseOverlayConfig.IsClickThru = this.MiniParseOverlayObsolete.IsClickThru;
                miniParseOverlayConfig.Position = this.MiniParseOverlayObsolete.Position;
                miniParseOverlayConfig.Size = this.MiniParseOverlayObsolete.Size;
                miniParseOverlayConfig.Url = this.MiniParseOverlayObsolete.Url;
                miniParseOverlayConfig.SortKey = this.MiniParseOverlayObsolete.SortKey;
                miniParseOverlayConfig.SortType = this.MiniParseOverlayObsolete.SortType;
                miniParseOverlayConfig.MaxFrameRate = this.MiniParseOverlayObsolete.MaxFrameRate;
                miniParseOverlayConfig.GlobalHotkey = this.MiniParseOverlayObsolete.GlobalHotkey;
                miniParseOverlayConfig.GlobalHotkeyEnabled = this.MiniParseOverlayObsolete.GlobalHotkeyEnabled;
                miniParseOverlayConfig.GlobalHotkeyModifiers = this.MiniParseOverlayObsolete.GlobalHotkeyModifiers;

                this.Overlays.RemoveAll(x => x.Name == miniParseOverlayConfig.Name);
                this.Overlays.Add(miniParseOverlayConfig);

                this.MiniParseOverlayObsolete = null;
            }
            if (this.SpellTimerOverlayObsolete != null)
            {
                var spellTimerOverlayConfig = new SpellTimerOverlayConfig(DefaultSpellTimerOverlayName);
                spellTimerOverlayConfig.IsVisible = this.SpellTimerOverlayObsolete.IsVisible;
                spellTimerOverlayConfig.IsClickThru = this.SpellTimerOverlayObsolete.IsClickThru;
                spellTimerOverlayConfig.Position = this.SpellTimerOverlayObsolete.Position;
                spellTimerOverlayConfig.Size = this.SpellTimerOverlayObsolete.Size;
                spellTimerOverlayConfig.Url = this.SpellTimerOverlayObsolete.Url;
                spellTimerOverlayConfig.MaxFrameRate = this.SpellTimerOverlayObsolete.MaxFrameRate;
                spellTimerOverlayConfig.GlobalHotkey = this.SpellTimerOverlayObsolete.GlobalHotkey;
                spellTimerOverlayConfig.GlobalHotkeyEnabled = this.SpellTimerOverlayObsolete.GlobalHotkeyEnabled;
                spellTimerOverlayConfig.GlobalHotkeyModifiers = this.SpellTimerOverlayObsolete.GlobalHotkeyModifiers;

                this.Overlays.RemoveAll(x => x.Name == spellTimerOverlayConfig.Name);
                this.Overlays.Add(spellTimerOverlayConfig);

                this.SpellTimerOverlayObsolete = null;
            }
#pragma warning restore 612, 618
        }
    }
}
