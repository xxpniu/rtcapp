
using System;

namespace RTC.XNet
{
    public enum VersionCompare
    {
        Equals,
        Less,
        Greater
    }
    public class Version
    {
        public int Major { set; get; }
        
        public int Minor { set; get; }
        
        public int Patch { set; get; }

        public Version(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public Version(string version)
        {
            var spl = version.Split('.');
            if (spl.Length != 3)
            {
                throw new ArgumentException($"Error Parser:{version}");
            }
            
            if (!int.TryParse(spl[0], out var major) 
                || !int.TryParse(spl[1], out var minor) 
                || !int.TryParse(spl[2], out var patch))
            {
                throw new InvalidCastException($"Error Parser:{version}");
            };
            
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public int Value => Major * 10000 + Minor * 100 + Patch;
        public static VersionCompare Compare(Version origin, Version compare )
        {
            if (origin.Value > compare.Value)
            {
                return VersionCompare.Greater;
            }
            return origin.Value == compare.Value ? VersionCompare.Equals : VersionCompare.Less;
        }

        public static explicit operator Version(string ver) => new Version(ver);


        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}