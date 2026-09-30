# Files and Paths

Load when the code touches `BinaryFile`, uploads, downloads, file handlers, the asset manager,
file paths, or zip and archive handling.

## Downloads

- **Repeat GetFile's checks.** Any endpoint that returns BinaryFile bytes does what GetFile
  does. It calls `binaryFile.ParentEntityAllowsView( person )` first. When there is no parent
  and the file type has `RequiresViewSecurity`, it requires `IsAuthorized( VIEW )` or a valid
  SecurityGrant.
- **Serve uploads safely.** Any endpoint that serves uploaded bytes adds
  `Content-Security-Policy: default-src 'none'; sandbox` and `X-Content-Type-Options: nosniff`,
  as GetFile and GetImage do. The stored MimeType came from the browser, so an HTML or SVG file
  would otherwise run script on the Rock domain. An image endpoint rejects files whose MimeType
  isn't `image/*`.
- **Async handlers** (`IHttpAsyncHandler`) resolve the current person from the request context
  they were given, not from `HttpContext.Current` inside an async continuation.

## Posted file references

- **Only the current file or a fresh upload.** A posted BinaryFile Id or Guid is accepted only
  if it is the record's current file or a temporary file this person uploaded:
  `new BinaryFileService( rockContext ).IsUploadedBinaryFileAllowedForPerson( postedId,
  currentId, currentPerson )`. The helper rejects non-temporary files, and it allows temporary
  files with no creator (anonymous uploads). If the uploader posts `IsTemporary=false`, check
  the creator, file type, and upload time directly (see `EmailForm.cs`). Without this, a person
  can attach someone else's file, and a later swap can mark it temporary so cleanup deletes it.
- **Parent links set on the server.** Every code path that creates or attaches a BinaryFile for
  a secured record sets `ParentEntityTypeId` and `ParentEntityId` on the server, so the file
  follows the parent's VIEW security. Never keep values the uploader posted. Once a parent is
  set, the file type's security no longer applies, so the parent must be the right record.
  Search every creation path.

## Uploads

- **File type lists.** Every new upload, extract, or rename path checks the file extension
  against the `ContentFiletypeBlacklist` and `ContentFiletypeWhitelist` global attributes, as
  `FileUploader.ValidateFileType` does. Writing an `.aspx`, `.ashx`, or `.config` file is code
  execution.

## Paths and archives

- **Stay inside the root.** Every posted path, uploaded file name, and archive entry is checked
  with `FileUtilities.IsPathWithinFolder( physicalPath, physicalRoot )` on the final physical
  path (after `MapPath` or `Path.Combine`) before read, delete, move, rename, or extract. This
  includes move and rename destinations and each extracted zip entry ("zip slip" is an entry
  like `..\..\web.config`).
- **Build paths from the issued root.** Asset manager and file browser endpoints build the path
  from the root the server issued plus a relative path, never from a raw posted path.
- **Protect the root itself.** The root folder can't be deleted, moved, or renamed.
  `IsPathWithinFolder` returns true for the root itself, so add a separate check.
