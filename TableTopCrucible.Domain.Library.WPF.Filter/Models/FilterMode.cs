using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TableTopCrucible.Domain.Library.WPF.Filter.Models
{
    public enum FilterMode
    {
        [Description("Include")]
        Whitelist,
        [Description("Exclude")]
        Blacklist
    }
    public enum StringFilterMode
    {
        [Description("Starts With")]
        StartsWith,
        [Description("Ends With")]
        EndsWith,
        [Description("Contains")]
        Contains,
        [Description("Advanced")]
        Advanced
    }
    public enum PathFilterComponent
    {
        [Description("Directory")]
        Directory,
        [Description("Path")]
        Path,
        [Description("File Name")]
        FileName
    }
    public enum CaseSensitivityMode
    {
        [Description("Respect")]
        RespectCase,
        [Description("Ignore")]
        IgnoreCase
    }
}
