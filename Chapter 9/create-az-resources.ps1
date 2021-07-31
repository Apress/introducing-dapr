$location = 'westeurope'
$resourceGroupName = 'images-detect-rg'
$storageAccountName = 'imgdetectstor'
$storageContainerName = 'images'
$storageQueueName = 'images'

Connect-AzAccount

$resourceGroup = Get-AzResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
if (-not $resourceGroup) {
    $resourceGroup = New-AzResourceGroup -Name $resourceGroupName -Location $location
}

$storageAccount = Get-AzStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountName -ErrorAction SilentlyContinue
if (-not $storageAccount) {
    $storageAccount = New-AzStorageAccount -ResourceGroupName $resourceGroupName -Name $storageAccountName -Location $location -SkuName Standard_LRS
}

$storageContainer = Get-AzStorageContainer -Name $storageContainerName -Context $storageAccount.Context -ErrorAction SilentlyContinue
if (-not $storageContainer) {
    $storageContainer = New-AzStorageContainer -Name $storageContainerName -Context $storageAccount.Context
}

$storageQueue = Get-AzStorageQueue -Name $storageQueueName -Context $storageAccount.Context -ErrorAction SilentlyContinue
if (-not $storageQueue) {
    $storageQueue = New-AzStorageQueue -Name $storageQueueName -Context $storageAccount.Context
}

$eventGridSub = Get-AzEventGridSubscription -EventSubscriptionName 'new-image' -ResourceId $storageAccount.Id -ErrorAction SilentlyContinue
if (-not $eventGridSub) {
    $newEventGridSubParams = @{
        EventSubscriptionName = 'new-image'
        ResourceId            = $storageAccount.Id
        EndpointType          = 'storagequeue'
        Endpoint              = "$($storageAccount.Id)/queueServices/default/queues/$storageQueueName"
        IncludedEventType     = @('Microsoft.Storage.BlobCreated')
        SubjectBeginsWith     = '/blobServices/default/containers/images/blobs/input'
        SubjectEndsWith       = '.jpg'
    }
    New-AzEventGridSubscription @newEventGridSubParams
}

$storageAccountKey = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName)[0].Value
Write-Output "Copy the following key and use it in the Dapr components: $storageAccountKey"