pipeline {
    agent any
    environment {
        PATH = "C:\\Program Files\\dotnet\\;${env.PATH}"
    }
    stages {
        stage('Build') {
            steps {
                bat '''
                    dotnet restore
                '''
                sh 'dotnet build --no-restore'
            }
        }
        stage('Test') {
            steps {
                sh 'dotnet test --no-build'
            }
        }
    }
}