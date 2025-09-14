# Audit Logs Dashboard

A modern, responsive web interface for viewing and managing audit logs from the Audit API.

## Features

- **Real-time Log Display**: View all audit logs with real-time updates
- **Advanced Filtering**: Filter logs by action, date range, and search text
- **Sorting Options**: Sort logs by date, action, or user
- **Responsive Design**: Works on desktop, tablet, and mobile devices
- **Statistics Dashboard**: View total logs, filtered logs, and unique actions
- **Auto-refresh**: Automatically refreshes every 30 seconds
- **Keyboard Shortcuts**: 
  - `Ctrl/Cmd + R`: Refresh logs
  - `Ctrl/Cmd + F`: Focus search input

## Usage

1. Start the Audit API service
2. Navigate to the root URL of the Audit API (e.g., `https://localhost:7001`)
3. The dashboard will automatically load and display all available logs

## Filtering and Search

- **Action Filter**: Select a specific action type from the dropdown
- **Search**: Search across log messages, actions, and user names
- **Date Range**: Filter logs by creation date using the date pickers
- **Sorting**: Choose how to sort the logs (by date, action, or user)
- **Clear Filters**: Reset all filters to show all logs

## API Endpoints Used

The frontend uses the following Audit API endpoints:
- `GET /api/logs` - Retrieve all logs
- `GET /api/logs/{id}` - Retrieve a specific log
- `GET /api/logs/user/{userId}` - Retrieve logs for a specific user
- `GET /api/logs/action/{action}` - Retrieve logs for a specific action

## Browser Compatibility

- Chrome 60+
- Firefox 55+
- Safari 12+
- Edge 79+

## Styling

The dashboard uses a modern glassmorphism design with:
- Gradient backgrounds
- Backdrop blur effects
- Smooth animations and transitions
- Responsive grid layouts
- Font Awesome icons

