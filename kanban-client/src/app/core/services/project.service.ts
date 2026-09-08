import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { ProjectDto, ProjectCreateDto, ProjectUpdateDto,ProjectMemberDto, AddProjectMemberDto } from '../../models/project.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private readonly apiUrl = `${environment.apiUrl}/projects`;

  constructor(private http: HttpClient) {}

  getMyProjects(): Observable<ProjectDto[]> {
    return this.http.get<ProjectDto[]>(this.apiUrl);
  }

  getProjectById(id: number): Observable<ProjectDto> {
    return this.http.get<ProjectDto>(`${this.apiUrl}/${id}`);
  }

  createProject(dto: ProjectCreateDto): Observable<ProjectDto> {
    return this.http.post<ProjectDto>(this.apiUrl, dto);
  }

  updateProject(id: number, dto: ProjectUpdateDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto);
  }

  deleteProject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getMembers(projectId: number): Observable<ProjectMemberDto[]> {
    return this.http.get<ProjectMemberDto[]>(`${this.apiUrl}/${projectId}/members`);
}

addMember(projectId: number, dto: AddProjectMemberDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${projectId}/members`, dto);
}

removeMember(projectId: number, memberUserId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${projectId}/members/${memberUserId}`);
}
}