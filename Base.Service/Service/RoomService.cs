using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public RoomService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        public async Task<ServiceResponseVM<Room>> Create(RoomVM newEntity)
        {
            var existedRoom = await _unitOfWork.RoomRepository.Get(r => r.RoomName.Equals(newEntity.RoomName)).FirstOrDefaultAsync();
            if (existedRoom is not null)
            {
                return new ServiceResponseVM<Room>
                {
                    IsSuccess = false,
                    Title = "Create Room failed",
                    Errors = new string[1] { "Room Name is already taken" }
                };

            }

            Room newRoom = new Room
            {
                RoomName = newEntity.RoomName,
                RoomDescription = newEntity.RoomDescription,
                RoomStatus = newEntity.RoomStatus,
                CreatedBy = _currentUserService.UserId,
                CreatedAt = ServerDateTime.GetVnDateTime(),
            };

            try
            {
                await _unitOfWork.RoomRepository.AddAsync(newRoom);

                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponseVM<Room>
                    {
                        IsSuccess = true,
                        Title = "Create Room successfully",
                        Result = newRoom
                    };
                }
                else
                {
                    return new ServiceResponseVM<Room>
                    {
                        IsSuccess = false,
                        Title = "Create Room failed",
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM<Room>
                {
                    IsSuccess = false,
                    Title = "Create Room failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM<Room>
                {
                    IsSuccess = false,
                    Title = "Create Room failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<ServiceResponseVM> Delete(int id)
        {
            var existedRoom = await _unitOfWork.RoomRepository.Get(r => r.RoomID == id && !r.IsDeleted).FirstOrDefaultAsync();
            if (existedRoom is null)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete room failed",
                    Errors = new string[1] { "Room not found" }
                };
            }

            existedRoom.IsDeleted = true;
            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = true,
                        Title = "Delete room successfully"
                    };
                }
                else
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = "Delete room failed",
                        Errors = new string[1] { "Save changes failed" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete room failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete room failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<IEnumerable<Room>> GetAll()
        {
            return await _unitOfWork.RoomRepository.Get(r => r.IsDeleted == false).ToArrayAsync();
        }

        public async Task<Room> GetByID(int id)
        {
            var room = await _unitOfWork.RoomRepository.FindAsync(id);
            return room!;
        }

        public async Task<ServiceResponseVM<Room>> Update(RoomVM updateEntity, int id)
        {
            var existedRoom = await _unitOfWork.RoomRepository.Get(r => r.RoomID == id).SingleOrDefaultAsync();
            if (existedRoom is null)
            {
                return new ServiceResponseVM<Room>
                {
                    IsSuccess = false,
                    Title = "Update Room failed",
                    Errors = new string[1] { "Room not found" }
                };

            }
            if(updateEntity.RoomName != existedRoom.RoomName)
            {
                var checkRoomName = await _unitOfWork.RoomRepository.Get(r => r.RoomName.Equals(updateEntity.RoomName)).SingleOrDefaultAsync();
                if (checkRoomName is not null)
                {
                    return new ServiceResponseVM<Room>
                    {
                        IsSuccess = false,
                        Title = "Update Room failed",
                        Errors = new string[1] { "Room Name is already taken" }
                    };

                }
            }

            existedRoom.RoomDescription = updateEntity.RoomDescription;
            existedRoom.RoomStatus = updateEntity.RoomStatus;
            existedRoom.RoomName = updateEntity.RoomName!;
            existedRoom.CreatedAt = ServerDateTime.GetVnDateTime();

            _unitOfWork.RoomRepository.Update(existedRoom);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                return new ServiceResponseVM<Room>
                {
                    IsSuccess = true,
                    Title = "Update Room successfully",
                    Result = existedRoom
                };
            }
            else
            {
                return new ServiceResponseVM<Room>
                {
                    IsSuccess = false,
                    Title = "Update Room failed"
                };
            }
        }
    }
}
