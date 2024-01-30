//-----------------------------------------------------------------------------
// Copyright 2023 FAST FORWARD LLC. All rights reserved.
//-----------------------------------------------------------------------------

namespace FastForward.CAS
{
    /// <summary>
    /// Returns a downloaded byte[] from Azure.
    /// </summary>
    /// <param name="success">Success or failure boolean.</param>
    /// <param name="error">Error text if applicable.</param>
    /// <param name="data">The data returned by the server.</param>
    public delegate void AzureDataDownloadCallback(bool success, string error, byte[] data);
}