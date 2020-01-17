namespace BBPalCalc.Platforms
{
    public static class PlatformFactory
    {
        public static IPlatform CreatePlatform(PlatformTypes platform)
        {
            switch (platform)
            {
                case PlatformTypes.AtariSte:
                    return new AtariStePlatform();
                case PlatformTypes.Amiga:
                    return new AmigaPlatform();
                case PlatformTypes.AtariSt:
                    return new AtariStPlatform();
                default:
                    return null;
            }
        }
    }
}
