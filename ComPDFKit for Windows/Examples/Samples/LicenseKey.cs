using ComPDFKit.NativeMethod;
using static ComPDFKit.NativeMethod.CPDFSDKVerifier;

public static class SDKLicenseHelper
{ 
    public static bool LicenseVerify()
    {
        if (!LoadNativeLibrary())
            return false;

        string license = "vRK+dzgqDsImQkoKZb2i2WhhdsKYquZ5JJUUuOF6q4xySFETAYvInXOu/f3JAvR9X14z/s4W20EE9WSKsbEum6IvdY8uLoNFy6YM7SyvNC0zVPs5T7GnEBaTQ+qVgnGeV88LA7nbZjPGfQmC75jFLlItpetrbbp75d7LQs6ftq/I8akPBz4Kxc3SSow2MFj5GzH6VbCFYSlUKw/TwA4adwM8uk2g6Kh86i23vTzGejQ4FtQ6stKHXn4HllO0bXpWnFihljcZ3R8PR31pFNwT8UUw4h9NVdFcZxpnGRDbwx9QP9WyHpHZLwbeO48ufCgFCVysJEk1riNSTGTAaAP3FUtTVN6c+2dRhnnKQ9BD2+ZXPq1h0mtlp1NQ+RMIko/jptqMsDODbroq9eCcyCqv15famjsc5QhApxJ66Uir6JIEWg+1gHSh2bjFiiXJAZ6NYxZRbQCMGNWAvkvPL3VOmCcPKDpJojB4dAuUzkjcfNP3FtGWASLlf1sxBLPPUH3/SUjuKo61mV+inIkdPNQcpTuQO57aUzB8KSNTD9t5EApfDx1B3KqboczEI8JHpWmS+IJqLCfsdlZLlGqIVobinsWoWlrK+RCjMLVb1nG6cwgJkaUZSOX6lhqKCVjsk/y/UkMufir2Jr3VOcSskcB2q89CFCFifHHHnhIhHClAv/cmsbjAv8udd4vC2c8/IMyeZDTKE61QPTGv5Web7Veww7UvwzOJJzdEyurazXA+8W54OEFOcXWYSdIgqS/vfVtnuO1kXkGQlSX1V0ead/9e5cgbJBA0v6oCmcgFsdJ19cJWN2eb8XRaylJBvRjCLTfxyhbz5ot00OuvKj41xr9kzFxfdbfnnZIUvdt0DhdKOcq/XESwVIL+pht7hkwzW7dk3HovfUURbW/42AUBTS3P/UNn7rp3eHOhZnTYo6erdzuLfnoMUa8M8nhOhIEkkRTxyCgKfwd4akkqZVJx9r83Hlf9ceP1IoDYgrXYwpetLJ8=";
        LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(license, false);
        return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
    }
}
