gcloud logging read "resource.type=""gce_instance"" AND textPayload:Exception" --limit=20 --format=json > gcp.json
