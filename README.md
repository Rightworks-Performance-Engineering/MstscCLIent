# MstscCLIent

MstscCLIent is a small Windows tool that provides an alternative RDP [connector](https://docs.loginvsi.com/login-enterprise/6.8/custom-connector) for [Login Enterprise](https://www.loginvsi.com/products/login-enterprise/)

## Features:
- Non-interactive RDP session creation
- Session screen capture
- Keyboard automation
- Prevent lock key writeback
- Syslog logging
- UI click protection

## Requirements:
- OS: Windows
- .NET Version: 10

## Example usage:
- Basic:
  ```powershell
  MstscCLIent.exe -d "DOMAIN" -u "user@domain" -p "P@ssw0rd" -f "rds-host.example.local"
  ```
- With Syslog:
  ```powershell
  MstscCLIent.exe -d "DOMAIN" -u "user@domain" -p "P@ssw0rd" -f "rds-host" -s "syslog.example.local"
  ```
- With debug logs:
  ```powershell
  MstscCLIent.exe -d "DOMAIN" -u "user@domain" -p "P@ssw0rd" -f "rds-host" -z
  ```
- LoginEnterprise Connector Command Line: 
  ```powershell
  "C:\Path\To\MstscCLIent.exe" -u "{username}" -p "{password}" -d "{domain}" -f "{host}" -s "SyslogServer.YourDomain.Com"
  ```

## Command line options:
- `-d, --Domain` (required) - Domain for connection.
- `-u, --Username` (required) - Username (e.g., `user@domain`).
- `-p, --Password` (required) - Password (sensitive; masked in logs).
- `-f, --FQDN` (required) - Host/FQDN to connect to.
- `-s, --Syslog` (optional) - Syslog server to send UDP logs.
- `-z, --Debug` (optional) - Enable debug file logging (writes to `%TEMP%` when enabled).

## Important runtime notes:
The app uses COM interop with MSTSC ActiveX, `RoyalApps.Community.Rdp.WinForms`, and low-level keyboard hooks.
  Running keyboard hooks or RDP automation may require administrative privileges and can trigger AV.

## Todo:
- [ ] Add support for [{SessionId}](https://docs.loginvsi.com/login-enterprise/6.8/custom-connector#Connection-Command-Line) command line argument for Login Enterprise 6.2+
- [ ] Implement [Named Pipes](https://docs.loginvsi.com/login-enterprise/6.4/configuring-connectors-and-connections#Named-Pipes-for-Custom-Connector) for Login Enterprise 6.4+ Custom Connectors using the [CustomConnector SDK](https://docs.loginvsi.com/login-enterprise/6.4/configuring-connectors-and-connections#CustomConnector-SDK-(Named-Pipe))
- [ ] Support resolutions other than 1920x1080

---

## Changelog:
See [CHANGELOG.md](CHANGELOG.md)

## Support Policy

This project is provided as an example and educational reference.

While issues and pull requests are welcome, there is no guarantee that reported issues, feature requests, or pull requests will be reviewed or addressed within any specific timeframe.

This project is provided without any support obligations, service level agreements (SLAs), warranties, or guarantees of any kind.

For legal warranty and liability terms, see [LICENSE](LICENSE) included with this project.

## License
This project is released under the AGPL-3.0 license. See [LICENSE](LICENSE).