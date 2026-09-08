import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { TaskDto, TaskCreateDto, TaskStatusUpdateDto } from '../../models/task.model';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getProjectTasks(projectId: number): Observable<TaskDto[]> {
    return this.http.get<TaskDto[]>(`${this.apiUrl}/projects/${projectId}/tasks`);
  }

  createTask(projectId: number, dto: TaskCreateDto): Observable<TaskDto> {
    return this.http.post<TaskDto>(`${this.apiUrl}/projects/${projectId}/tasks`, dto);
  }

  updateTaskStatus(taskId: number, status: string): Observable<void> {
    const dto: TaskStatusUpdateDto = { status };
    return this.http.patch<void>(`${this.apiUrl}/tasks/${taskId}/status`, dto);
  }

  deleteTask(taskId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}`);
  }
}