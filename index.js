module.exports = async function (context, req) {
    context.log('Processing request for Azure pricing data');

    // Input validation
    if (!req.body) {
        return badRequest(context, "Request body is required");
    }

    const { serviceType, region, skuName } = req.body;

    if (!serviceType || !region) {
        return badRequest(context, "Service type and region are required");
    }

    try {
        // Configure Retail API request
        const retailApiUrl = process.env.RETAIL_API_ENDPOINT || 'https://api.retail.microsoft.com';
        const apiVersion = '2023-01-01-preview';
        const endpoint = `${retailApiUrl}/prices/${apiVersion}/?currencyCode=USD&$filter=`;

        // Build filter based on service type
        let filter = `serviceName eq '${mapServiceTypeToServiceName(serviceType)}' and armRegionName eq '${region}'`;
        
        if (skuName) {
            filter += ` and armSkuName eq '${skuName}'`;
        }

        // Make request to Retail API
        const fetch = require('node-fetch');
        const response = await fetch(encodeURI(endpoint + filter), {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`Retail API returned ${response.status}: ${await response.text()}`);
        }

        const data = await response.json();
        
        // Return the pricing data
        context.res = {
            status: 200,
            body: data.Items || [],
            headers: { 'Content-Type': 'application/json' }
        };

    } catch (error) {
        context.log.error('Error fetching pricing data:', error);
        
        context.res = {
            status: 500,
            body: { error: "Failed to retrieve pricing data", details: error.message },
            headers: { 'Content-Type': 'application/json' }
        };
    }
};

function badRequest(context, message) {
    context.res = {
        status: 400,
        body: { error: message },
        headers: { 'Content-Type': 'application/json' }
    };
    return;
}

function mapServiceTypeToServiceName(serviceType) {
    // Map the frontend service type values to actual Azure service names used in the Retail API
    const serviceMap = {
        'virtualmachines': 'Virtual Machines',
        'storage': 'Storage',
        'databases': 'SQL Database',
        'appservice': 'App Service',
        'networking': 'Virtual Network'
    };
    
    return serviceMap[serviceType] || serviceType;
}