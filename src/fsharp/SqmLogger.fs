//----------------------------------------------------------------------------
//
// Copyright (c) 2002-2012 Microsoft Corporation. 
//
// This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
// copy of the license can be found in the License.html file at the root of this distribution. 
// By using this source code in any fashion, you are agreeing to be bound 
// by the terms of the Apache License, Version 2.0.
//
// You must not remove this notice, or any other, from this software.
//----------------------------------------------------------------------------

module internal Microsoft.FSharp.Compiler.SqmLogger

open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.Build
open Microsoft.FSharp.Compiler.ErrorLogger
open Internal.Utilities 

module internal SqmWrapper =
    open System.Runtime.InteropServices
    open System
 
    [<ComImport; Interface>]
    [<Guid("B17A7D4A-C1A3-45A2-B916-826C3ABA067E") ; InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
    type IVsSqmMulti =
        abstract GetOptInStatus : unit -> bool
        abstract UnloadSessions : unit -> unit
        abstract EndAllSessionsAndAbortUploads : unit -> unit
        abstract BeginSession : 
                [<In; MarshalAs(UnmanagedType.U4)>] sessionType : System.UInt32 * 
                [<In; MarshalAs(UnmanagedType.VariantBool)>] alwaysSend : System.Boolean * 
                [<Out; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 byref -> unit 
        abstract EndSession : [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 -> unit
        abstract RegisterSessionHandle :
                [<In; MarshalAs(UnmanagedType.LPStruct)>] sessionIdentifier : System.Guid *
                [<In; MarshalAs(UnmanagedType.U4)>] dwSessionHandle : System.UInt32 -> unit
        abstract GetSessionHandleByIdentifier :
                [<In; MarshalAs(UnmanagedType.LPStruct)>] sessionIdentifier : System.Guid *
                [<Out; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 byref -> unit
        abstract GetSessionStartTime :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<Out>] time : System.Runtime.InteropServices.ComTypes.FILETIME byref -> unit
        abstract GetGlobalSessionGuid : unit -> Guid
        abstract GetGlobalSessionHandle : [<Out; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 byref -> unit
        abstract SetGlobalSessionGuid : [<MarshalAs(UnmanagedType.LPStruct)>] pguidSessionGuid : System.Guid -> unit
        abstract GetFlags:
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<Out; MarshalAs(UnmanagedType.U4)>] flags : System.UInt32 byref -> unit
        abstract SetFlags:
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32  *
                [<In; MarshalAs(UnmanagedType.U4)>] flags : System.UInt32  -> unit
        abstract ClearFlags : 
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32  *
                [<In; MarshalAs(UnmanagedType.U4)>] flags : System.UInt32  -> unit
        abstract SetDatapoint : 
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] value : System.UInt32 -> unit
        abstract SetBoolDatapoint :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32  *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32  *
                [<In; MarshalAs(UnmanagedType.U4)>] fValue : System.UInt32 -> unit
        abstract SetStringDatapoint :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.BStr)>] strValue : string -> unit
        abstract SetDatapointBits :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] value : System.UInt32 -> unit
        abstract IncrementDatapoint :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] value : System.UInt32 -> unit
        abstract SetDatapointIfMax :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] value : System.UInt32 -> unit
        abstract SetDatapointIfMin :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] value : System.UInt32 -> unit
        abstract AddToDatapointAverage :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] value : System.UInt32 -> unit
        abstract StartDatapointTimer :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 -> unit
        abstract RecordDatapointTimer :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 -> unit
        abstract AccumulateDatapointTimer :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 -> unit
        abstract AddTimerToDatapointAverage :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 -> unit
        abstract AddItemToStream :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] value : System.UInt32 -> unit
        abstract AddArrayToStream :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4, SizeParamIndex = 2s)>] data : System.UInt32[] *
                [<In; MarshalAs(UnmanagedType.I4)>] count : int -> unit
        abstract AddToStreamDWord :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] cTuple : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] value : System.UInt32 -> unit
        abstract AddToStreamString :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] cTuple : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.BStr)>] strValue : string -> unit
        abstract RecordCmdData :
                [<In; MarshalAs(UnmanagedType.U4)>] sessionHandle : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.LPStruct)>] pguidCmdGroup : System.Guid *
                [<In; MarshalAs(UnmanagedType.U4)>] dataPointID : System.UInt32 *
                [<In; MarshalAs(UnmanagedType.U4)>] value : System.UInt32 -> unit
        abstract GetHashOfGuid :
                [<In; MarshalAs(UnmanagedType.LPStruct)>] hashGuid : System.Guid *
                [<Out; MarshalAs(UnmanagedType.U4)>] resultantHash : System.UInt32 ref -> unit
        abstract GetHashOfString :
                [<In; MarshalAs(UnmanagedType.BStr)>] hashString : string *
                [<Out; MarshalAs(UnmanagedType.U4)>] resultantHash : System.UInt32 ref -> unit

    [<DllImport("FSharp.VsSqmMulti.dll", SetLastError=true, EntryPoint="QueryService")>]
    let QuerySqmService(
                        ([<MarshalAs(UnmanagedType.LPStruct)>] _clsidguid : System.Guid),
                        ([<MarshalAs(UnmanagedType.LPStruct)>] _guid : System.Guid),
                        ([<MarshalAs(UnmanagedType.Interface)>] _metaHost : IVsSqmMulti byref)) : int = failwith "QuerySqmService"

    let GetSqmService() =
        let mutable sqm = Unchecked.defaultof<IVsSqmMulti>
        let mutable rsid = new Guid("2508FDF0-EF80-4366-878E-C9F024B8D981") // IVsLog Interface
        let riid = new System.Guid("B17A7D4A-C1A3-45A2-B916-826C3ABA067E")  // IVsSqmMulti Interface
        let ret = QuerySqmService(rsid, riid, &sqm)
        match ret with
        | 0 (* NOERROR *) -> sqm
        | _ -> raise <| Exception (sprintf "QuerySqmService fails with errorcode=%d" ret)

    // Define SQM-related datapoints
    // App Id (SqmSessionType)
    let FSHARP_APPID = 40u 

    // Only including datapoints we are actually using in FSharp
    let DATAID_SQM_FSC_BUILDVERSION = 1262u
    let DATAID_SQM_FSC_CONFIG = 1263u
    let DATAID_SQM_FSC_SOURCES = 1264u
    let DATAID_SQM_FSC_COMPILETIME = 1265u
    let DATAID_SQM_FSC_FXVERSION = 1266u
    let DATAID_SQM_FSC_FSLIBVERSION = 1267u
    let DATAID_SQM_FSC_REFERENCES = 1268u
    let DATAID_SQM_FSC_ERRORS = 1269u

let getRootDirectoryName (path:string) =
    try 
        let root = System.IO.DirectoryInfo path
        if root.Exists then root.Name
        else "NA"
    with _ -> "NA"
                        
let SqmLoggerWithConfigBuilder (tcConfigB : TcConfigBuilder) (errors : int list) =    
    /// SQM logs are generated only when it is in fsc.exe called by Visual Studio.    
    if not tcConfigB.isInteractive && Option.isSome tcConfigB.sqmSessionGuid then

        try
            let sqm = SqmWrapper.GetSqmService()

            // Update GlobalSessionGuid
            match tcConfigB.sqmSessionGuid with
            | Some guid -> sqm.SetGlobalSessionGuid(guid)
            | None -> ()

            // Start Session
            let sqmSession = sqm.BeginSession(SqmWrapper.FSHARP_APPID, false)
    
            // Log Build Version
            let buildVersion = FSharpEnvironment.DotNetBuildString
            sqm.SetStringDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_BUILDVERSION, buildVersion)

            // Log PrimaryAssembly, Platform, & Target
            let primaryAssembly = match tcConfigB.primaryAssembly with | Mscorlib  -> 0x1u | NamedMscorlib _ -> 0x2u | DotNetCore -> 0x3u
            let platform = match tcConfigB.platform with | None -> 0x0u | Some ILPlatform.X86 -> 0x1u | Some ILPlatform.AMD64 -> 0x2u | Some ILPlatform.IA64 -> 0x3u
            let target = match tcConfigB.target with | WinExe -> 0x0u | ConsoleExe -> 0x1u | Dll -> 0x2u | Module -> 0x3u
            let value = (primaryAssembly <<< 16) ||| (platform <<< 8) ||| target           
            sqm.SetDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_CONFIG, value)

            // Log Errors
            errors |> List.iter (fun err -> sqm.AddItemToStream(sqmSession, SqmWrapper.DATAID_SQM_FSC_ERRORS, (uint32) err))

            // Log Project complexity      
            sqm.SetDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_SOURCES, (uint32)tcConfigB.sqmNumOfSourceFiles) 
              
            // Close session
            sqm.EndSession(sqmSession)

        with err ->
            System.Diagnostics.Debug.Assert(false, sprintf "Could not get SQM service or have SQM related errors: %s" (err.ToString()))
         
let SqmLoggerWithConfig (tcConfig : TcConfig) (errors : int list) =    
    /// SQM logs are generated only when it is in fsc.exe called by Visual Studio.    
    if not tcConfig.isInteractive && Option.isSome tcConfig.sqmSessionGuid then   
        try
            let sqm = SqmWrapper.GetSqmService()

            // Update GlobalSessionGuid
            match tcConfig.sqmSessionGuid with
            | Some guid -> sqm.SetGlobalSessionGuid(guid)
            | None -> ()

            // Start Session
            let sqmSession = sqm.BeginSession(SqmWrapper.FSHARP_APPID, false)
    
            // Log Compilation Time
            let elapsed = System.DateTime.Now - System.DateTime(tcConfig.sqmSessionStartedTime)
            sqm.SetDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_COMPILETIME, (uint32)elapsed.TotalMilliseconds)

            // Log Build Version
            let buildVersion = FSharpEnvironment.DotNetBuildString
            sqm.SetStringDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_BUILDVERSION, buildVersion)

            // Log PrimaryAssembly, Platform & Target
            let primaryAssembly = match tcConfig.primaryAssembly with | Mscorlib  -> 0x1u | NamedMscorlib _ -> 0x2u | DotNetCore -> 0x3u
            let platform = match tcConfig.platform with | None -> 0x0u | Some ILPlatform.X86 -> 0x1u | Some ILPlatform.AMD64 -> 0x2u | Some ILPlatform.IA64 -> 0x3u
            let target = match tcConfig.target with | WinExe -> 0x0u | ConsoleExe -> 0x1u | Dll -> 0x2u | Module -> 0x3u
            let value = (primaryAssembly <<< 16) ||| (platform <<< 8) ||| target           
            sqm.SetDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_CONFIG, value)

            // Log Framework & Fslib Version
            let frameworkVersion = match tcConfig.ClrRoot with [dir] -> getRootDirectoryName dir | _ -> "NA"
            sqm.SetStringDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_FXVERSION, frameworkVersion)
            
            let fslibVersion = getRootDirectoryName tcConfig.fsharpBinariesDir
            sqm.SetStringDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_FSLIBVERSION, fslibVersion)

            // Log Project complexity      
            let frameworkDLLs, nonFrameworkReferences, _ = TcAssemblyResolutions.SplitNonFoundationalResolutions tcConfig
            sqm.SetDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_REFERENCES, (uint32)frameworkDLLs.Length <<< 16 ||| (uint32)nonFrameworkReferences.Length)
            sqm.SetDatapoint(sqmSession, SqmWrapper.DATAID_SQM_FSC_SOURCES, (uint32)tcConfig.sqmNumOfSourceFiles) 

            // Log Errors
            errors |> List.iter (fun err -> sqm.AddItemToStream(sqmSession, SqmWrapper.DATAID_SQM_FSC_ERRORS, (uint32) err))
              
            // Close session
            sqm.EndSession(sqmSession)

        with err ->
            System.Diagnostics.Debug.Assert(false, sprintf "Could not get SQM service or have SQM related errors: %s" (err.ToString()))