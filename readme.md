# Azure Price Estimator

A web application to calculate and compare Azure service prices including Virtual Machines and Storage options.

## Features

- Calculate Azure service prices based on your requirements
- Compare Pay-As-You-Go and Reserved Instance pricing
- Support for multiple currencies (EUR, USD, GBP)
- Export results to Excel
- Storage pricing with different redundancy options

## Usage

1. Select your region (EU West, EU North, etc.)
2. Choose a service family (Compute or Storage)
3. Select specific service options:
   - For VMs: Choose size (D2s v3, D4s v3, etc.)
   - For Storage: Select redundancy (LRS, ZRS, GRS)
4. Set your usage period:
   - Monthly (730h)
   - Weekly (168h)
   - Daily (24h)
   - Partial Day (8h)
5. Click "Calculate Prices" to see results

## Development

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or Visual Studio Code

### Dependencies

- ASP.NET Core 8.0
- ClosedXML (for Excel export)
- Bootstrap 5.x
- Bootstrap Icons

## Configuration

The application uses Azure Retail Prices API to fetch current pricing information. No authentication is required.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
