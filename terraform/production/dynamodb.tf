resource "aws_dynamodb_table" "tenureinformationapi_dynamodb_table" {
    name                  = "TenureInformation"
    billing_mode          = "PROVISIONED"
    read_capacity         = 10
    write_capacity        = 10
    hash_key              = "id"

    attribute {
        name              = "id"
        type              = "S"
    }

    tags = {
        Name              = "tenure-information-api-${var.environment_name}"
        Environment       = var.environment_name
        terraform-managed = true
        project_name      = var.project_name
        backup_policy     = "Prod"
    }
    
    point_in_time_recovery {
        enabled           = true
    }
}
