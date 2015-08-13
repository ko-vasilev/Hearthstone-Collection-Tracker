using System;
using System.Globalization;

namespace Hearthstone_Collection_Tracker.Internal
{
    /// <summary>
    /// Serializable version of the System.Version class.
    /// </summary>
    [Serializable]
    public class ModuleVersion : ICloneable, IComparable
    {
        private int major;
        private int minor;
        private int build;
        private int revision;
        /// <summary>
        /// Gets the major.
        /// </summary>
        /// <value></value>
        public int Major
        {
            get
            {
                return major;
            }
            set
            {
                major = value;
            }
        }
        /// <summary>
        /// Gets the minor.
        /// </summary>
        /// <value></value>
        public int Minor
        {
            get
            {
                return minor;
            }
            set
            {
                minor = value;
            }
        }
        /// <summary>
        /// Gets the build.
        /// </summary>
        /// <value></value>
        public int Build
        {
            get
            {
                return build;
            }
            set
            {
                build = value;
            }
        }
        /// <summary>
        /// Gets the revision.
        /// </summary>
        /// <value></value>
        public int Revision
        {
            get
            {
                return revision;
            }
            set
            {
                revision = value;
            }
        }
        /// <summary>
        /// Creates a new <see cref="ModuleVersion"/> instance.
        /// </summary>
        public ModuleVersion()
        {
            this.build = -1;
            this.revision = -1;
            this.major = 0;
            this.minor = 0;
        }
        /// <summary>
        /// Creates a new <see cref="ModuleVersion"/> instance.
        /// </summary>
        /// <param name="version">Version.</param>
        public ModuleVersion(string version)
        {
            this.build = -1;
            this.revision = -1;
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            char[] chArray1 = new char[1] { '.' };
            string[] textArray1 = version.Split(chArray1);
            int num1 = textArray1.Length;
            if ((num1 < 2) || (num1 > 4))
            {
                throw new ArgumentException("Arg_VersionString");
            }
            this.major = int.Parse(textArray1[0], CultureInfo.InvariantCulture);
            if (this.major < 0)
            {
                throw new ArgumentOutOfRangeException("version", "ArgumentOutOfRange_Version");
            }
            this.minor = int.Parse(textArray1[1], CultureInfo.InvariantCulture);
            if (this.minor < 0)
            {
                throw new ArgumentOutOfRangeException("version", "ArgumentOutOfRange_Version");
            }
            num1 -= 2;
            if (num1 > 0)
            {
                this.build = int.Parse(textArray1[2], CultureInfo.InvariantCulture);
                if (this.build < 0)
                {
                    throw new ArgumentOutOfRangeException("build", "ArgumentOutOfRange_Version");
                }
                num1--;
                if (num1 > 0)
                {
                    this.revision = int.Parse(textArray1[3], CultureInfo.InvariantCulture);
                    if (this.revision < 0)
                    {
                        throw new ArgumentOutOfRangeException("revision", "ArgumentOutOfRange_Version");
                    }
                }
            }
        }
        /// <summary>
        /// Creates a new <see cref="ModuleVersion"/> instance.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        public ModuleVersion(int major, int minor)
        {
            this.build = -1;
            this.revision = -1;
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException("major", "ArgumentOutOfRange_Version");
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException("minor", "ArgumentOutOfRange_Version");
            }
            this.major = major;
            this.minor = minor;
            this.major = major;
        }
        /// <summary>
        /// Creates a new <see cref="ModuleVersion"/> instance.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        /// <param name="build">Build.</param>
        public ModuleVersion(int major, int minor, int build)
        {
            this.build = -1;
            this.revision = -1;
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException("major", "ArgumentOutOfRange_Version");
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException("minor", "ArgumentOutOfRange_Version");
            }
            if (build < 0)
            {
                throw new ArgumentOutOfRangeException("build", "ArgumentOutOfRange_Version");
            }
            this.major = major;
            this.minor = minor;
            this.build = build;
        }
        /// <summary>
        /// Creates a new <see cref="ModuleVersion"/> instance.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        /// <param name="build">Build.</param>
        /// <param name="revision">Revision.</param>
        public ModuleVersion(int major, int minor, int build, int revision)
        {
            this.build = -1;
            this.revision = -1;
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException("major", "ArgumentOutOfRange_Version");
            }
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException("minor", "ArgumentOutOfRange_Version");
            }
            if (build < 0)
            {
                throw new ArgumentOutOfRangeException("build", "ArgumentOutOfRange_Version");
            }
            if (revision < 0)
            {
                throw new ArgumentOutOfRangeException("revision", "ArgumentOutOfRange_Version");
            }
            this.major = major;
            this.minor = minor;
            this.build = build;
            this.revision = revision;
        }
        /// <summary>
        /// Creates a new <see cref="ModuleVersion"/> instance using existing <see cref="Version"/> object.
        /// </summary>
        /// <param name="version">Object which properties will be used for copying</param>
        public ModuleVersion(Version version)
        {
            this.build = version.Build;
            this.revision = version.Revision;
            this.major = version.Major;
            this.minor = version.Minor;
            this.build = version.Build;
            this.revision = version.Revision;
        }
        #region ICloneable Members
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            ModuleVersion version1 = new ModuleVersion();
            version1.major = this.major;
            version1.minor = this.minor;
            version1.build = this.build;
            version1.revision = this.revision;
            return version1;
        }
        #endregion
        #region IComparable Members
        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="obj">Obj.</param>
        /// <returns></returns>
        public int CompareTo(object version)
        {
            if (version == null)
            {
                return 1;
            }
            if (!(version is ModuleVersion))
            {
                throw new ArgumentException("Arg_MustBeVersion");
            }
            ModuleVersion version1 = (ModuleVersion)version;
            if (this.major != version1.Major)
            {
                if (this.major > version1.Major)
                {
                    return 1;
                }
                return -1;
            }
            if (this.minor != version1.Minor)
            {
                if (this.minor > version1.Minor)
                {
                    return 1;
                }
                return -1;
            }
            if (this.build != version1.Build)
            {
                if (this.build > version1.Build)
                {
                    return 1;
                }
                return -1;
            }
            if (this.revision == version1.Revision)
            {
                return 0;
            }
            if (this.revision > version1.Revision)
            {
                return 1;
            }
            return -1;
        }
        #endregion
        /// <summary>
        /// Equalss the specified obj.
        /// </summary>
        /// <param name="obj">Obj.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is ModuleVersion))
            {
                return false;
            }
            ModuleVersion version1 = (ModuleVersion)obj;
            if (((this.major == version1.Major) && (this.minor == version1.Minor)) && (this.build == version1.Build) && (this.revision == version1.Revision))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int num1 = 0;
            num1 |= ((this.major & 15) << 0x1c);
            num1 |= ((this.minor & 0xff) << 20);
            num1 |= ((this.build & 0xff) << 12);
            return (num1 | this.revision & 0xfff);
        }
        /// <summary>
        /// Operator ==s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator ==(ModuleVersion v1, ModuleVersion v2)
        {
            return v1.Equals(v2);
        }
        /// <summary>
        /// Operator &gt;s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator >(ModuleVersion v1, ModuleVersion v2)
        {
            return (v2 < v1);
        }
        /// <summary>
        /// Operator &gt;=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator >=(ModuleVersion v1, ModuleVersion v2)
        {
            return (v2 <= v1);
        }
        /// <summary>
        /// Operator !=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator !=(ModuleVersion v1, ModuleVersion v2)
        {
            return (v1 != v2);
        }
        /// <summary>
        /// Operator &lt;s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator <(ModuleVersion v1, ModuleVersion v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException("v1");
            }
            return (v1.CompareTo(v2) < 0);
        }
        /// <summary>
        /// Operator &lt;=s the specified v1.
        /// </summary>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        /// <returns></returns>
        public static bool operator <=(ModuleVersion v1, ModuleVersion v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException("v1");
            }
            return (v1.CompareTo(v2) <= 0);
        }
        /// <summary>
        /// Toes the string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.build == -1)
            {
                return this.ToString(2);
            }
            if (this.revision == -1)
            {
                return this.ToString(3);
            }
            return this.ToString(4);
        }
        /// <summary>
        /// Toes the string.
        /// </summary>
        /// <param name="fieldCount">Field count.</param>
        /// <returns></returns>
        public string ToString(int fieldCount)
        {
            object[] objArray1;
            switch (fieldCount)
            {
                case 0:
                    {
                        return string.Empty;
                    }
                case 1:
                    {
                        return (this.major.ToString());
                    }
                case 2:
                    {
                        return (this.major.ToString() + "." + this.minor.ToString());
                    }
            }
            if (this.build == -1)
            {
                throw new ArgumentException(string.Format("ArgumentOutOfRange_Bounds_Lower_Upper {0},{1}", "0", "2"), "fieldCount");
            }
            if (fieldCount == 3)
            {
                objArray1 = new object[5] { this.major, ".", this.minor, ".", this.build };
                return string.Concat(objArray1);
            }
            if (this.revision == -1)
            {
                throw new ArgumentException(string.Format("ArgumentOutOfRange_Bounds_Lower_Upper {0},{1}", "0", "3"), "fieldCount");
            }
            if (fieldCount == 4)
            {
                objArray1 = new object[7] { this.major, ".", this.minor, ".", this.build, ".", this.revision };
                return string.Concat(objArray1);
            }
            throw new ArgumentException(string.Format("ArgumentOutOfRange_Bounds_Lower_Upper {0},{1}", "0", "4"), "fieldCount");
        }
    }
}