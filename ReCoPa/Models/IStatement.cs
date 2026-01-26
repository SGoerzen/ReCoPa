using System;

namespace ReCoPa.Models;

public interface IStatement
{
    string Id { get; }
    string Content { get; }
    DateTime Timestamp { get; }
}