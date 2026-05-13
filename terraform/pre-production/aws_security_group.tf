data "aws_vpc" "pre_production_vpc" {
  tags = {
    Name = "housing-pre-prod-pre-prod"
  }
}

resource "aws_security_group" "lambda" {
  name                   = "tenure-information-api-lambda-sg"
  description            = "Security group used by the tenure API lambda function"
  vpc_id                 = data.aws_vpc.pre_production_vpc.id
  revoke_rules_on_delete = true
}

resource "aws_security_group_rule" "egress_https" {
  security_group_id = aws_security_group.lambda.id
  type              = "egress"
  cidr_blocks       = ["0.0.0.0/0"]
  description       = "Allow egress on port 443"
  from_port         = 443
  to_port           = 443
  protocol          = "TCP"
}
