document.addEventListener('DOMContentLoaded', function() {
    // DOM Elements
    const serviceTypeSelect = document.getElementById('service-type');
    const regionSelect = document.getElementById('region');
    const vmSizeContainer = document.getElementById('vm-size-container');
    const vmSizeSelect = document.getElementById('vm-size');
    const storageTypeContainer = document.getElementById('storage-type-container');
    const storageTypeSelect = document.getElementById('storage-type');
    const searchButton = document.getElementById('search-prices');
    const loadingElement = document.getElementById('loading');
    const errorMessage = document.getElementById('error-message');
    const resultsTable = document.getElementById('results-table');
    const resultsBody = document.getElementById('results-body');

    // Azure Function App URL - replace with your actual function URL
    const functionAppBaseUrl = "https://func-yourfunctionapp.azurewebsites.net/api/";

    // Event Listeners
    serviceTypeSelect.addEventListener('change', handleServiceTypeChange);
    searchButton.addEventListener('click', fetchPrices);

    // Functions
    function handleServiceTypeChange() {
        // Reset and hide all conditional inputs
        vmSizeContainer.style.display = 'none';
        storageTypeContainer.style.display = 'none';

        // Show relevant inputs based on selected service
        const selectedService = serviceTypeSelect.value;
        
        if (selectedService === 'virtualmachines') {
            vmSizeContainer.style.display = 'block';
        } else if (selectedService === 'storage') {
            storageTypeContainer.style.display = 'block';
        }
    }

    async function fetchPrices() {
        // Hide previous results and errors, show loading
        resultsTable.classList.add('hidden');
        errorMessage.classList.add('hidden');
        loadingElement.classList.remove('hidden');
        
        // Validate inputs
        const serviceType = serviceTypeSelect.value;
        const region = regionSelect.value;
        
        if (!serviceType || !region) {
            showError('Please select both a service type and region.');
            return;
        }

        // Build request parameters
        let requestParams = {
            serviceType: serviceType,
            region: region
        };

        // Add service-specific parameters
        if (serviceType === 'virtualmachines' && vmSizeSelect.value) {
            requestParams.skuName = vmSizeSelect.value;
        } else if (serviceType === 'storage' && storageTypeSelect.value) {
            requestParams.skuName = storageTypeSelect.value;
        }

        try {
            // Determine which endpoint to call based on the service type
            let endpoint = `${functionAppBaseUrl}GetAzurePrices`;
            
            // Make API call
            const response = await fetch(endpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestParams)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }

            const data = await response.json();
            displayResults(data);
        } catch (error) {
            console.error('Error fetching prices:', error);
            showError('Failed to fetch pricing data. Please try again later.');
        } finally {
            loadingElement.classList.add('hidden');
        }
    }

    function displayResults(priceData) {
        // Clear previous results
        resultsBody.innerHTML = '';
        
        if (!priceData || priceData.length === 0) {
            showError('No pricing data available for the selected options.');
            return;
        }

        // Add each price to the table
        priceData.forEach(item => {
            const row = document.createElement('tr');
            
            row.innerHTML = `
                <td>${item.serviceName || 'N/A'}</td>
                <td>${item.skuName || 'N/A'}</td>
                <td>${item.region || 'N/A'}</td>
                <td>${formatCurrency(item.retailPrice)}</td>
                <td>${item.unitOfMeasure || 'N/A'}</td>
            `;
            
            resultsBody.appendChild(row);
        });

        // Show the results table
        resultsTable.classList.remove('hidden');
    }

    function showError(message) {
        loadingElement.classList.add('hidden');
        errorMessage.textContent = message;
        errorMessage.classList.remove('hidden');
    }

    function formatCurrency(value) {
        if (value === null || value === undefined) return 'N/A';
        return `$${parseFloat(value).toFixed(4)}`;
    }
});