# FileSystemMonitor
A C# wrapper for monitoring file system changes with event-driven and queue-based processing options.

# Features
- Monitor file and directory changes in real-time
- Support for recursive subdirectory monitoring
- File pattern filtering (e.g., ".txt", ".log")
- Dual processing modes: event callbacks or queue-based polling
- Thread-safe event queue with lock protection
- Track creates, deletes, changes, renames, and errors
- Implements IDisposable for proper resource cleanup

# Event Types
- Create: New file or directory created
- Delete: File or directory deleted
- Change: File or directory modified
- Rename: File or directory renamed
- Error: Monitoring error occurred

# Use Cases
- File synchronization tools
- Log file monitoring and analysis
- Automated backup systems
- Build system file watchers
- Development tool file change detection
- Document management systems

# Requirements
- .NET Framework 4.5+ or .NET Core/5+
- File system read permissions for monitored paths
