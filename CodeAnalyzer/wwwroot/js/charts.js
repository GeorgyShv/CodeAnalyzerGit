// Функции для создания различных типов графиков

function createHalsteadChart(containerId, data) {
    const ctx = document.getElementById(containerId).getContext('2d');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Длина программы (N)', 'Объем программы (V)', 'Потенциальный объем (V*)'],
            datasets: [{
                label: 'Метрики Холстеда',
                data: [data.programLength, data.programVolume, data.potentialVolume],
                backgroundColor: [
                    'rgba(54, 162, 235, 0.5)',
                    'rgba(255, 99, 132, 0.5)',
                    'rgba(75, 192, 192, 0.5)'
                ],
                borderColor: [
                    'rgb(54, 162, 235)',
                    'rgb(255, 99, 132)',
                    'rgb(75, 192, 192)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

function createGilbChart(containerId, data) {
    const ctx = document.getElementById(containerId).getContext('2d');
    new Chart(ctx, {
        type: 'radar',
        data: {
            labels: ['Абсолютная сложность', 'Относительная сложность', 'Количество модулей', 'Связи между модулями'],
            datasets: [{
                label: 'Метрики Джилба',
                data: [
                    data.absoluteComplexity,
                    data.relativeComplexity,
                    data.moduleCount,
                    data.moduleConnections
                ],
                backgroundColor: 'rgba(255, 99, 132, 0.2)',
                borderColor: 'rgb(255, 99, 132)',
                pointBackgroundColor: 'rgb(255, 99, 132)',
                pointBorderColor: '#fff',
                pointHoverBackgroundColor: '#fff',
                pointHoverBorderColor: 'rgb(255, 99, 132)'
            }]
        },
        options: {
            responsive: true,
            scales: {
                r: {
                    beginAtZero: true
                }
            }
        }
    });
}

function createChepinChart(containerId, data) {
    const ctx = document.getElementById(containerId).getContext('2d');
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ['Вводимые (P)', 'Модифицируемые (M)', 'Управляющие (C)', 'Паразитные (T)'],
            datasets: [{
                data: [
                    data.inputVariables,
                    data.modifiedVariables,
                    data.controlVariables,
                    data.parasiticVariables
                ],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.5)',
                    'rgba(54, 162, 235, 0.5)',
                    'rgba(255, 206, 86, 0.5)',
                    'rgba(75, 192, 192, 0.5)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)',
                    'rgb(54, 162, 235)',
                    'rgb(255, 206, 86)',
                    'rgb(75, 192, 192)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    text: 'Распределение переменных по метрике Чепина'
                }
            }
        }
    });
}

function createErrorPredictionChart(containerId, data) {
    const ctx = document.getElementById(containerId).getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.volumes,
            datasets: [{
                label: 'Предсказанное количество ошибок',
                data: data.errors,
                fill: false,
                borderColor: 'rgb(75, 192, 192)',
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Количество ошибок'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Объем программы'
                    }
                }
            }
        }
    });
} 