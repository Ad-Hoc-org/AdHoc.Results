// Copyright AdHoc Authors
// SPDX-License-Identifier: MIT

Console.WriteLine("Hello, World!");

static Results<Success<string>, InvalidData, NotFound, Unauthorized> ReadFile(string path)
{
    if (path is null)
        return path.InvalidData();
    if (!File.Exists(path))
        return path.NotFound();
    try
    {
        return File.ReadAllText(path).Success();
    }
    catch (PathTooLongException ex)
    {
        return path.InvalidData(exception: ex);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Unauthorized(ex);
    }
    catch (FileNotFoundException ex)
    {
        return path.NotFound(exception: ex);
    }
    catch (DirectoryNotFoundException ex)
    {
        return path.NotFound(exception: ex);
    }
}

static Results<Success<int>, InvalidData, Unprocessable, Unauthorized> ReadConfig(string path) =>
    ReadFile(path).Variant switch
    {
        Success<string>(var value) =>
            int.TryParse(value, out var result) ? result.Success() : path.Unprocessable(),
        NotFound notFound => Unprocessable(notFound.Message, notFound),
        var variant => new(variant)
    };
