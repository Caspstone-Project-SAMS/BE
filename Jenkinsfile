pipeline {
    agent {label "linux"}
    stages {
        stage('Deploy on host machine') {
            steps {
                script {
                    sh '''
                        docker rm -f sams-be || true
                        docker pull khoale111/sams-project:latest11
                        docker run --name sams-be -p 80:80 -p 8080:8080 -p 8081:8081 -p 444:444 -p 443:443 -d khoale111/sams-project:latest11
                    '''
                }
            }
        }
    }
}