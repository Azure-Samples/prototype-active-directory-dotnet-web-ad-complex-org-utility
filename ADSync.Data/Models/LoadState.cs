﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ADSync.Data.Models
{
    /// <summary>
    /// PendingHQAdd        - Remote site has just uploaded this new user for creation at HQ
    /// PendingRemoteUpdate - HQ has updated with HQGUID, Remote site needs to pick up and update local AD
    /// PendingHQUpdate     - Remote site has updated the user, HQ should update HQ AD (not implemented)
    /// NothingPending      - HQ sets this after a PendingHQUpdate, and Remote site sets this after a PendingRemoteUpdate
    /// PendingHQDelete     - Remote site has deleted the user, HQ should delete the user from HQ AD (not implemented)
    /// Deleted             - User has been deleted at Remote site and HQ (not implemented)
    /// </summary>
    public enum LoadStage
    {
        PendingHQAdd,
        PendingRemoteUpdate,
        PendingHQUpdate,
        NothingPending,
        PendingHQDelete,
        Deleted
    }
}