#r "nuget: SharpCompress, 0.29.0"

open System
open System.IO
open System.Net.Http
open SharpCompress.Common
open SharpCompress.Readers

let crate = "wasmer-c-api"
let version = "3.3.0"

let createDir (path: string) =
    if not <| Directory.Exists path then Directory.CreateDirectory path |> ignore

let downloadAndExtract (url: string) (destination: string) =
    use client = new HttpClient()
    let tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
    let response = client.GetAsync(url).Result
    response.EnsureSuccessStatusCode() |> ignore
    let bytes = response.Content.ReadAsByteArrayAsync().Result
    createDir destination
    File.WriteAllBytes(tempFile, bytes)
    if File.Exists(tempFile) then
        use stream = File.OpenRead(tempFile)
        use reader = ReaderFactory.Open(stream)
        while reader.MoveToNextEntry() do
            if not reader.Entry.IsDirectory then
                let options = ExtractionOptions()
                options.ExtractFullPath <- true
                options.Overwrite <- true
                reader.WriteEntryToDirectory(destination, options)

let moveDir (src: string) (dest: string) =
    if Directory.Exists src then Directory.Move(src, dest)

let removeDir (path: string) =
    if Directory.Exists path then Directory.Delete(path, true)

let baseUrl = "https://crates.io/api/v1/crates"

let downloadUrl = $"%s{baseUrl}/%s{crate}/%s{version}/download"
downloadAndExtract downloadUrl $"include/%s{crate}-%s{version}"