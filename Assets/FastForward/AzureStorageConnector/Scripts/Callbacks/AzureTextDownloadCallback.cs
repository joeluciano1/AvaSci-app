//-----------------------------------------------------------------------------
// Copyright 2023 FAST FORWARD LLC. All rights reserved.
//-----------------------------------------------------------------------------

namespace FastForward.CAS
{
    /// <summary>
    /// Returns information about Azure.
    /// </summary>
    /// <param name="success">Success or failure boolean.</param>
    /// <param name="error">Error text if applicable.</param>
    /// <param name="text">The text returned by the server.</param>
    public delegate void AzureTextDownloadCallback(bool success, string error, string text);
}