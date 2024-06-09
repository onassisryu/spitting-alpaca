pipeline {
    agent any
   
    stages {
        stage('프론트 배포') {
            steps {								
                dir("Spitting-Alpaca") {
                    sh '''
                    rm -rf /data/front/*
                    mv WebGLBuild/* /data/front/
                    '''
                }
            }
            post {
                success {
                    echo "배포 성공"
                }
                failure {
                    echo "배포 실패"
                }
            }
        }
    }
}