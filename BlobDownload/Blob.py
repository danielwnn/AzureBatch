import re
import argparse
from azure.storage.blob import BlobServiceClient

# create parser
parser = argparse.ArgumentParser()
 
# add arguments to the parser
parser.add_argument("blob_url")
 
# parse the arguments
args = parser.parse_args()
blob_url = args.blob_url
print(blob_url)

blob_info = blob_url.replace('https://', '').replace('/', '#', 2).split('#')

container_name = blob_info[1]
print(container_name)

blob_name = blob_info[2]
print(blob_name)

blob_service_client = BlobServiceClient.from_connection_string("<connection string>")

# Instantiate a new ContainerClient
container_client = blob_service_client.get_container_client(container_name)

# Instantiate a new BlobClient
blob_client = container_client.get_blob_client(blob_name)

# download a blob
with open("C:\tmp\LACapital.png", "wb") as my_blob:
  download_stream = blob_client.download_blob()
  my_blob.write(download_stream.readall())

