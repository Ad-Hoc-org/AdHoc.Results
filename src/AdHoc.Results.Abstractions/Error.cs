using System.Collections.Immutable;
using System.Diagnostics;
using AdHoc.Results.Abstractions;

namespace AdHoc.Results;

public abstract record Error : IError
{
    bool IResult.IsSuccess => false;

    public abstract string? Type { get; }
    public abstract string? Message { get; init; }
    public abstract Exception? Exception { get; init; }
    public abstract ImmutableArray<IError> Errors { get; init; }

#if FEATURE_TRACE
    public string? File { get; }
    public int Line { get; }
    public string? Method { get; }

    protected Error()
    {
        var trace = new StackTrace(1, true);
        var type = GetType();
        if (type is { IsConstructedGenericType: true })
            type = type.GetGenericTypeDefinition();
        var typeName = type.FullName!;
        bool IsConstructor(StackFrame frame)
        {
            var method = DiagnosticMethodInfo.Create(frame);
            return method is not null &&
                method.Name == ".ctor" && method.DeclaringTypeName == typeName;
        }

        var frames = trace.GetFrames();
        var i = 0;
        // find first constructor frame for this type
        while (i < frames.Length)
        {
            var frame = frames[i];
            if (IsConstructor(frame))
                break;
            i++;
        }
        // find frame outside constructor of this type
        while (++i < frames.Length)
        {
            var frame = frames[i];
            if (!IsConstructor(frame))
                break;
        }

        if (i < frames.Length)
        {
            var frame = frames[i];
            File = frame.GetFileName();
            Line = frame.GetFileLineNumber();
            var method = DiagnosticMethodInfo.Create(frame);
            if (method is not null)
                Method = method.DeclaringTypeName + '.' + method.Name;
        }
    }
#endif
}
