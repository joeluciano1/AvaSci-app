//-----------------------------------------------------------------------------
// Copyright 2023 FAST FORWARD LLC. All rights reserved.
//-----------------------------------------------------------------------------

namespace FastForward.CAS
{
    /// <summary>
    /// Returns the result of an upload to Azure call.  
    /// </summary>
    /// <param name="success">Success or failure boolean.</param>
    /// <param name="error">Error text if applicable.</param>
    /// <param name="uri">If the call succeeded, this is the location where the blob was placed.</param>
    public delegate void AzureUploadCallback(bool success, string error, string uri = null);
}